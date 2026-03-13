using System;
using System.Collections.Generic;

namespace Shared.Contracts.Read
{
    public class LicenseRead
    {
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
        public LicenseType? LicenseType { get; set; }
        public LicenseTier? LicenseTier { get; set; }
        public string CustomerName { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationCustomName { get; set; }
        public string ApplicationCustomDomain { get; set; }
        public DateTime ApplicationRegistered { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? DaysRemaining { get; set; }
        public int MaxBeacons { get; set; }
        public int MaxReaders { get; set; }
        public CategorizedFeaturesRead Features { get; set; }
    }

    public class CategorizedFeaturesRead
    {
        public Dictionary<string, FeatureItemRead> Core { get; set; } = new Dictionary<string, FeatureItemRead>();
        public Dictionary<string, FeatureItemRead> Modules { get; set; } = new Dictionary<string, FeatureItemRead>();
    }

    public class FeatureItemRead
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
    }
}
