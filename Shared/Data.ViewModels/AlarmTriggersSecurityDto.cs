using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    /// <summary>
    /// DTO for Security actions on Alarm Triggers
    /// Used by security personnel to accept, arrive, and complete investigation
    /// </summary>
    public class AlarmTriggersSecurityActionDto
    {
        /// <summary>
        /// Required for DoneInvestigated action - the investigation result text
        /// </summary>
        [StringLength(4000)]
        public string? InvestigationResult { get; set; }
    }
}
