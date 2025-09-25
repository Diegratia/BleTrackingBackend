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
        Cancelled
    }

    public enum IdentityType
    {
        NIK,
        KTP,
        Passport
    }

    public enum CardType
    {
        Rfid,
        Ble,
        RfidBle
    }

    public enum IntegrationType
    {
        Sdk,
        Api,
        Other
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
        Other
        // RatherNotSay
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
        Idle,
        Done,
        NoAction,
        Waiting,
        Investigated,
        DoneInvestigated,
        PostponeInvestigated
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
        Stop
    }
    public enum AlarmLevelPriority
    {
        High,
        Medium,
        Low
    }
}