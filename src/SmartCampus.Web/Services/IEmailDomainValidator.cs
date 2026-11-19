using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;

namespace SmartCampus.Web.Services;

public interface IEmailDomainValidator
{
    Task<(bool IsAllowed, string? ErrorMessage, string DefaultRole)> ValidateDomainAsync(string email);
}

public class EmailDomainValidator : IEmailDomainValidator
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public EmailDomainValidator(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool IsAllowed, string? ErrorMessage, string DefaultRole)> ValidateDomainAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return (false, "Invalid email address format.", "StudentTeacher");
        }

        var domain = email.Split('@')[1].ToLowerInvariant().Trim();

        // 1. Query database table AllowedEmailDomains
        var allowedDomain = await _context.AllowedEmailDomains
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DomainName.ToLower() == domain && d.IsActive);

        if (allowedDomain != null)
        {
            return (true, null, allowedDomain.DefaultRole);
        }

        // 2. Check development environment override
        var allowAnyInDev = _configuration.GetValue<bool>("SmartCampusSettings:AllowAnyDomainInDev");
        if (allowAnyInDev)
        {
            return (true, null, "StudentTeacher");
        }

        return (false, $"The email domain '@{domain}' is not authorized. Please use your official institutional email (@lpu.co.in or @lpu.in).", "StudentTeacher");
    }
}
