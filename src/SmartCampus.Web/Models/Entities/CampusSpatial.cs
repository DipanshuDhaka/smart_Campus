using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class Building
{
    [Key]
    public int BuildingId { get; set; }

    [Required]
    [MaxLength(20)]
    public string BuildingCode { get; set; } = string.Empty; // e.g. "BLK-32", "BH-01", "LIB"

    [Required]
    [MaxLength(150)]
    public string BuildingName { get; set; } = string.Empty; // e.g. "School of Computer Science & Engineering"

    public BuildingZoneType ZoneType { get; set; } = BuildingZoneType.Academic;

    public int TotalFloors { get; set; } = 1;

    [Column(TypeName = "decimal(9, 6)")]
    public decimal Latitude { get; set; } = 31.253600m;

    [Column(TypeName = "decimal(9, 6)")]
    public decimal Longitude { get; set; } = 75.703700m;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}

public class Floor
{
    [Key]
    public int FloorId { get; set; }

    public int BuildingId { get; set; }

    [ForeignKey(nameof(BuildingId))]
    public Building? Building { get; set; }

    public int FloorNumber { get; set; } // -1=Basement, 0=Ground, 1..N

    [Required]
    [MaxLength(50)]
    public string FloorName { get; set; } = string.Empty; // e.g. "Ground Floor", "3rd Floor"

    [MaxLength(500)]
    public string? FloorPlanUrl { get; set; }

    public ICollection<Location> Locations { get; set; } = new List<Location>();
}

public class Location
{
    [Key]
    public int LocationId { get; set; }

    public int FloorId { get; set; }

    [ForeignKey(nameof(FloorId))]
    public Floor? Floor { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoomNumber { get; set; } = string.Empty; // e.g. "32-401", "Lab-2", "Washroom-G3"

    [Required]
    [MaxLength(150)]
    public string LocationName { get; set; } = string.Empty; // e.g. "Advanced Computing & AI Lab"

    public LocationType LocationType { get; set; } = LocationType.Classroom;

    public Guid LocationGuid { get; set; } = Guid.NewGuid(); // Unique identifier for dynamic QR code URL

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }

    public bool HasQRCode { get; set; } = true;

    [MaxLength(500)]
    public string? QRCodeImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
