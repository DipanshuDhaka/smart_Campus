using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IQRService _qrService;

    public LocationApiController(ApplicationDbContext context, IQRService qrService)
    {
        _context = context;
        _qrService = qrService;
    }

    [HttpGet("buildings")]
    public async Task<IActionResult> GetBuildings()
    {
        var buildings = await _context.Buildings
            .Where(b => b.IsActive)
            .OrderBy(b => b.BuildingCode)
            .Select(b => new
            {
                b.BuildingId,
                b.BuildingCode,
                b.BuildingName,
                b.ZoneType,
                b.Latitude,
                b.Longitude,
                b.TotalFloors
            })
            .ToListAsync();

        return Ok(buildings);
    }

    [HttpGet("floors/{buildingId:int}")]
    public async Task<IActionResult> GetFloors(int buildingId)
    {
        var floors = await _context.Floors
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.FloorNumber)
            .Select(f => new
            {
                f.FloorId,
                f.FloorNumber,
                f.FloorName
            })
            .ToListAsync();

        return Ok(floors);
    }

    [HttpGet("locations/{floorId:int}")]
    public async Task<IActionResult> GetLocations(int floorId)
    {
        var locations = await _context.Locations
            .Where(l => l.FloorId == floorId && l.IsActive)
            .OrderBy(l => l.RoomNumber)
            .Select(l => new
            {
                l.LocationId,
                l.RoomNumber,
                l.LocationName,
                l.LocationType,
                l.LocationGuid
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("resolve-qr/{guid:guid}")]
    public async Task<IActionResult> ResolveQr(Guid guid)
    {
        var location = await _context.Locations
            .Include(l => l.Floor)
                .ThenInclude(f => f!.Building)
            .FirstOrDefaultAsync(l => l.LocationGuid == guid && l.IsActive);

        if (location == null)
        {
            return NotFound(new { message = "Invalid or inactive campus QR location code." });
        }

        return Ok(new
        {
            location.LocationId,
            location.RoomNumber,
            location.LocationName,
            LocationType = location.LocationType.ToString(),
            FloorId = location.FloorId,
            FloorName = location.Floor?.FloorName,
            BuildingId = location.Floor?.BuildingId,
            BuildingCode = location.Floor?.Building?.BuildingCode,
            BuildingName = location.Floor?.Building?.BuildingName,
            Latitude = location.Latitude ?? location.Floor?.Building?.Latitude,
            Longitude = location.Longitude ?? location.Floor?.Building?.Longitude
        });
    }

    [HttpGet("qr-image/{guid:guid}")]
    public async Task<IActionResult> GetQrImage(Guid guid)
    {
        var location = await _context.Locations
            .Include(l => l.Floor)
                .ThenInclude(f => f!.Building)
            .FirstOrDefaultAsync(l => l.LocationGuid == guid);

        if (location == null)
        {
            return NotFound();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var reportUrl = $"{baseUrl}/Reporter/Complaints/Create?locId={guid}";
        var base64Qr = _qrService.GenerateBase64QrCode(reportUrl);

        return Ok(new
        {
            LocationGuid = guid,
            LocationName = $"{location.Floor?.Building?.BuildingCode} - {location.RoomNumber} ({location.LocationName})",
            TargetUrl = reportUrl,
            Base64Image = $"data:image/png;base64,{base64Qr}"
        });
    }

    [HttpGet("campus-markers")]
    public async Task<IActionResult> GetCampusMarkers()
    {
        var buildings = await _context.Buildings
            .Include(b => b.Floors)
                .ThenInclude(f => f.Locations)
                    .ThenInclude(l => l.Complaints)
            .Where(b => b.IsActive)
            .ToListAsync();

        var markers = buildings.Select(b =>
        {
            var activeComplaints = b.Floors
                .SelectMany(f => f.Locations)
                .SelectMany(l => l.Complaints)
                .Count(c => c.Status != Models.Enums.ComplaintStatus.VerifiedResolved && 
                            c.Status != Models.Enums.ComplaintStatus.Closed && 
                            c.Status != Models.Enums.ComplaintStatus.Rejected);

            return new
            {
                b.BuildingId,
                b.BuildingCode,
                b.BuildingName,
                Zone = b.ZoneType.ToString(),
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                ActiveIssues = activeComplaints
            };
        });

        return Ok(markers);
    }
}
