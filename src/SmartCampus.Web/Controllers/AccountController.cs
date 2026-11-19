using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailDomainValidator _domainValidator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context,
        IEmailDomainValidator domainValidator,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _domainValidator = domainValidator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", email);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToRoleDashboard();
        }

        ModelState.AddModelError(string.Empty, "Invalid login credentials. Please verify your institutional email and password.");
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        if (!schemes.Any(s => s.Name.Equals(provider, StringComparison.OrdinalIgnoreCase)))
        {
            // Fallback for evaluation when Google ClientId/Secret are not yet configured in appsettings
            return RedirectToAction(nameof(SimulatedGoogleLogin), new { returnUrl });
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult SimulatedGoogleLogin(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessSimulatedGoogleLogin(string googleEmail, string? fullName, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(googleEmail))
        {
            ModelState.AddModelError(string.Empty, "Google institutional email is required.");
            ViewData["ReturnUrl"] = returnUrl;
            return View("SimulatedGoogleLogin");
        }

        // 1. Validate institutional domain whitelist
        var (isAllowed, domainError, defaultRole) = await _domainValidator.ValidateDomainAsync(googleEmail);
        if (!isAllowed)
        {
            TempData["ErrorMessage"] = domainError;
            return RedirectToAction(nameof(Login));
        }

        // 2. Check if user already exists
        var user = await _userManager.FindByEmailAsync(googleEmail);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = googleEmail,
                Email = googleEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }

            await _userManager.AddToRoleAsync(user, defaultRole);

            _context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FullName = fullName ?? googleEmail.Split('@')[0],
                RegistrationNumber = "GOOGLE-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                IsApprovedByAdmin = true
            });
            await _context.SaveChangesAsync();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User {Email} authenticated via Google Identity Provider.", googleEmail);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToRoleDashboard();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"External provider authentication error: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "Failed to load external login authentication claims from Google.";
            return RedirectToAction(nameof(Login));
        }

        // 1. Extract verified email and name claims
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email?.Split('@')[0] ?? "Campus User";

        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Google account did not return a verified email address.";
            return RedirectToAction(nameof(Login));
        }

        // 2. Validate institutional domain whitelist
        var (isAllowed, domainError, defaultRole) = await _domainValidator.ValidateDomainAsync(email);
        if (!isAllowed)
        {
            TempData["ErrorMessage"] = domainError;
            return RedirectToAction(nameof(Login));
        }

        // 3. Check if user already exists
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Auto-provision student/teacher identity account
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }

            // Assign default authorized role
            await _userManager.AddToRoleAsync(user, defaultRole);

            // Create UserProfile
            _context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FullName = name,
                IsApprovedByAdmin = true
            });
            await _context.SaveChangesAsync();
        }

        // 4. Link Google login if not already linked
        var logins = await _userManager.GetLoginsAsync(user);
        if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            await _userManager.AddLoginAsync(user, info);
        }

        // 5. Sign in
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("User {Email} logged in via Google OAuth 2.0.", email);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToRoleDashboard();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    public IActionResult RedirectToRoleDashboard()
    {
        if (User.IsInRole("SuperAdmin") || User.IsInRole("CampusAdmin"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        if (User.IsInRole("DepartmentSupervisor"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Supervisor" });
        }
        if (User.IsInRole("MaintenanceWorker"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Worker" });
        }

        return RedirectToAction("Index", "Dashboard", new { area = "Reporter" });
    }
}
