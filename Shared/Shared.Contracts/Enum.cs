
using System.Text.Json.Serialization;

namespace Shared.Contracts
{
    public enum OrganizationType
    {
        Single,
        Small,
        Medium,
        Big,
        Corporate,
        Government
    }

    public enum ReaderType
    {
        Indoor,
        Outdoor
    }

    public enum ApplicationType
    {
        Empty,
        Vms,
        Smr,
        Signage,
        Parking,
        Automation,
        Tracking
    }

    public enum AccessScope
    {
        All,
        Specific,
        None
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LicenseType
    {
        Trial, // trial 7 hari
        Perpetual, // permanent
        Annual // tahunan
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LicenseTier
    {
        Core, // 20 beacon, 5 reader
        Professional, // 200 beacon // 50 reader
        Enterprise, // unlimited
        Custom
    }

    public enum VisitorActiveStatus
    {
        Active,
        Expired,
        Cancelled,
        Extended
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IdentityType
    {
        DriverLicense,
        KTP,
        Passport,
        CardAccess,
        Face,
        NDA,
        Other,
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SwapMode
    {
        CardSwap,
        HoldIdentity,
        CardAndIdentity,
        ExtendAccess,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SwapType
    {
        EnterArea,
        ExitArea
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardSwapStatus
    {
        Pending,    // Transaksi dibuat tapi belum dieksekusi
        Active,     // Sedang menahan kartu (kartu di lokasi)
        Completed,  // Sudah dikembalikan/reverse
        Cancelled   // Transaksi batal
    }

    public enum PersonType
    {
        RegulerEmployee,
        Suspect,
        Highthreat,
        VIP,
        Staff,
        Contractor,
        Delivery,
        Visitor
    }

    public enum CardType
    {
        Rfid,
        Ble,
        RfidBle
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardStatus
    {
        Available,
        Used,
        Lost,
        Damaged,
        Swapped
    }


    public enum IntegrationType
    {
        Sdk,
        Api,
        Other
    }

    public enum ImagePurpose
    {
        Photo,
        Floorplan
    }


    public enum ApiTypeAuth
    {
        Basic,
        Bearer,
        ApiKey
    }

    public enum Gender
    {
        Male,
        Female,
        // Otther
        RatherNotSay
    }
    public enum ScheduleType
    {
        Shift,
        Patrol,
    }

    public enum StatusEmployee
    {
        Active,
        NonActive,
        Mutation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusActive
    {
        NonActive,
        Active

    }

    public enum RestrictedStatus
    {
        Restrict,
        NonRestrict
    }

    public enum VisitorStatus
    {
        Waiting,
        Checkin,
        Checkout,
        Denied,
        Block,
        Unblock,
        Precheckin,
        Preregist
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AlarmStatus
    {
        NonActive,
        Active
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AlarmRecordStatus
    {
        Block,
        Help,
        WrongZone,
        Expired,
        Lost,
        Blacklist,
        Geofence,
        OverPopulating,
        StayOnArea,
        Boundary,
        CardAccess
    }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActionStatus
    {
        Idle,               // default - alarm triggered by system
        Acknowledged,       // operator acknowledged the alarm
        Dispatched,         // operator dispatched to security
        Waiting,            // operator put in waiting queue (no security available)
        Accepted,           // security accepted the dispatch
        Arrived,            // security arrived at location
        DoneInvestigated,   // security submitted investigation result
        Done,               // operator marked as resolved
        NoAction,           // operator marked as no action needed
        PostponeInvestigated // operator postponed investigation (1 week)
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceType
    {
        Cctv,
        AccessDoor,
        BleReader
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceStatus
    {
        Active,
        NonActive,
        Damaged,
        Close,
        Open,
        Monitor,
        Alarm
    }

    public enum ServiceStatus
    {
        Start,
        Stop,
        Online,
        Offline
    }
    public enum AlarmLevelPriority
    {
        High,
        Medium,
        Low
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LevelPriority
    {
        System,
        SuperAdmin,
        PrimaryAdmin, // operator or security head
        Primary, //security / Guard
        Secondary, // member /employee
        UserCreated // visitor
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvestigatedResult
    {
        TestTrigger,
        FalseAlarm,
        AuthorizedAccess,
        SuspiciousActivity,
        TrepassingWarning,
        EscortedOut,
        Apprehended,
        HandedOverToPolice,
        EscalatedToLawEnforcement,
        Other
    }

    public enum BoundaryType
    {
        Both, // 0
        AtoB, // 1
        BtoA // 2
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolApprovalType
    {
        ByThreatLevel, //ThreatLevel
        WithoutApproval,
        Or,
        And,
        Sequential,
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CaseApprovalType // auto by threat level
    {
        WithoutApproval,
        Or, // default
        And,
        Sequential,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ThreatLevel
    {
        None,
        Low, // without approval
        Medium, // or
        High, // and
        Critical, // sequential
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CaseStatus
    {
        Open,
        Submitted,
        Approved,
        Rejected,
        Closed
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CaseType
    {
        Incident,
        Hazard,
        Damage,
        Theft,
        Report, // other
        PatrolSummary 
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileType
    {
        Image,
        Video,
        Document
    }

        public enum PeakHoursGroupByMode
    {
        Area = 0,
        Building = 1,
        Floor = 2,
        Floorplan = 3
    }

        public enum AlarmGroupByMode
    {
        Area = 0,
        Building = 1,
        Floor = 2,
        Floorplan = 3
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EvacuationAlertStatus
    {
        Active,
        Completed,
        Cancelled
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EvacuationTriggerType
    {
        Manual,
        FireAlarm,
        Earthquake,
        OtherEmergency
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EvacuationPersonStatus
    {
        Remaining,
        Evacuated,
        Confirmed
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EvacuationPersonCategory
    {
        Member,
        Visitor,
        Security
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolCheckpointStatus
    {
        AutoDetected = 0,
        Cleared = 1,
        HasCase = 2,
        Missed = 3
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolDurationType
    {
        NoDuration = 0,
        WithDuration = 1
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolStartType
    {
        Manual = 0,
        AutoStart = 1
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolCycleType
    {
        HalfCycle = 0, // A-B-C -> C-B-A 2 cycle
        FullCycle = 1  // A-B-C -> C-B-A 1 cycle
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PatrolTrackingMode
    {
        Auto = 0,     // BLE engine tracks automatically
        Manual = 1  // Default - requires manual checkpoint submission
    }
}
