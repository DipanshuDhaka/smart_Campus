using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class BuildingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IQRService _qrService;

    public BuildingsController(ApplicationDbContext context, IQRService qrService)
    {
        _context = context;
        _qrService = qrService;
    }

    public async Task<IActionResult> Index()
    {
        var buildings = await _context.Buildings
            .Include(b => b.Floors)
                .ThenInclude(f => f.Locations)
            .OrderBy(b => b.BuildingCode)
            .ToListAsync();

        return View(buildings);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Building { Latitude = 31.2536m, Longitude = 75.7037m });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Building building)
    {
        if (ModelState.IsValid)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            // Auto-create Ground Floor by default
            _context.Floors.Add(new Floor
            {
                BuildingId = building.BuildingId,
                FloorNumber = 0,
                FloorName = "Ground Floor"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Building {building.BuildingCode} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(building);
    }

    [HttpGet]
    public async Task<IActionResult> Floors(int id)
    {
        var building = await _context.Buildings
            .Include(b => b.Floors)
                .ThenInclude(f => f.Locations)
            .FirstOrDefaultAsync(b => b.BuildingId == id);

        if (building == null) return NotFound();

        return View(building);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFloor(int buildingId, int floorNumber, string floorName)
    {
        if (string.IsNullOrWhiteSpace(floorName))
        {
            TempData["ErrorMessage"] = "Floor name cannot be empty.";
            return RedirectToAction(nameof(Floors), new { id = buildingId });
        }

        var floor = new Floor
        {
            BuildingId = buildingId,
            FloorNumber = floorNumber,
            FloorName = floorName
        };

        _context.Floors.Add(floor);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Floor '{floorName}' added successfully.";
        return RedirectToAction(nameof(Floors), new { id = buildingId });
    }

    [HttpGet]
    public async Task<IActionResult> Locations(int floorId)
    {
        var floor = await _context.Floors
            .Include(f => f.Building)
            .Include(f => f.Locations)
            .FirstOrDefaultAsync(f => f.FloorId == floorId);

        if (floor == null) return NotFound();

        return View(floor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLocation(int floorId, string roomNumber, string locationName, LocationType locationType)
    {
        if (string.IsNullOrWhiteSpace(roomNumber) || string.IsNullOrWhiteSpace(locationName))
        {
            TempData["ErrorMessage"] = "Room number and location name are required.";
            return RedirectToAction(nameof(Locations), new { floorId });
        }

        var loc = new Location
        {
            FloorId = floorId,
            RoomNumber = roomNumber,
            LocationName = locationName,
            LocationType = locationType,
            LocationGuid = Guid.NewGuid(),
            HasQRCode = true
        };

        _context.Locations.Add(loc);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Location '{roomNumber} - {locationName}' added with dynamic QR Code.";
        return RedirectToAction(nameof(Locations), new { floorId });
    }
}
