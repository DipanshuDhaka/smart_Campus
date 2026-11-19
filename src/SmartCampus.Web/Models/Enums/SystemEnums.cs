namespace SmartCampus.Web.Models.Enums;

public enum ComplaintStatus
{
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Assigned = 5,
    WorkAccepted = 6,
    InProgress = 7,
    WorkCompleted = 8,
    AwaitingVerification = 9,
    VerifiedResolved = 10,
    ReworkRequired = 11,
    Closed = 12,
    Reopened = 13
}

public enum PriorityLevel
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

public enum WorkOrderStatus
{
    Assigned = 1,
    WorkAccepted = 2,
    InProgress = 3,
    WorkCompleted = 4,
    AwaitingVerification = 5,
    VerifiedResolved = 6,
    ReworkRequired = 7
}

public enum EscalationTier
{
    Tier1_Reminder = 1,  // 50% elapsed
    Tier2_Warning = 2,   // 75% elapsed
    Tier3_Breach = 3     // 100% breached
}

public enum WorkerAvailability
{
    Available = 1,
    Busy = 2,
    Overloaded = 3,
    OnLeave = 4
}

public enum LocationType
{
    Classroom = 1,
    ComputerLab = 2,
    EngineeringWorkshop = 3,
    LectureHall = 4,
    FacultyCabin = 5,
    Washroom = 6,
    CorridorStairwell = 7,
    HostelRoom = 8,
    OutdoorPathway = 9,
    ParkingArea = 10,
    UtilitySubstation = 11,
    CafeteriaFoodCourt = 12
}

public enum BuildingZoneType
{
    Academic = 1,
    ResidentialHostel = 2,
    Administrative = 3,
    Commercial = 4,
    Recreational = 5,
    UtilityInfrastructure = 6
}
