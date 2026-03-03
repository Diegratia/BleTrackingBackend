using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class AlarmCategorySettingsFilter : BaseFilter
    {
        public int? IsEnabled { get; set; }
    }
}
