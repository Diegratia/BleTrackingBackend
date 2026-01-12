namespace Helpers.Consumer
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

    public enum LicenseType
    {
        Perpetual,
        Annual
    }

    public enum VisitorActiveStatus
    {
        Active,
        Expired,
        Cancelled,
        Extended
    }

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

        public enum SwapType
    {
        EnterArea,  
        ExitArea 
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

    public enum StatusEmployee
    {
        Active,
        NonActive,
        Mutation
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

    public enum AlarmStatus
    {
        NonActive,
        Active
    }

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

    public enum ActionStatus
    {
        Idle, // default
        Done, // sudah selesai
        NoAction, // tidak ada tindakan
        Waiting, // satpam habis menunggu giliran
        Investigated, // 5 menit
        PostponeInvestigated // 1 minggu 
    }

    public enum DeviceType
    {
        Cctv,
        AccessDoor,
        BleReader
    }

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

    public enum LevelPriority
    {
        System,
        SuperAdmin,
        PrimaryAdmin,
        Primary,
        Secondary,
        UserCreated
    }

    public enum BoundaryType
    {
        Both, // 0
        AtoB, // 1
        BtoA // 2
    }
}