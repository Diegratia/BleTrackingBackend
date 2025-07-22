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

    public enum LicenseType
    {
        Perpetual,
        Annual
    }

    public enum VisitorType
    {
        Active,
        Expired,
        Cancelled
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
        Lost
    }

    public enum ActionStatus
    {
        Idle,
        Done,
        Cancel,
        Need,
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
}