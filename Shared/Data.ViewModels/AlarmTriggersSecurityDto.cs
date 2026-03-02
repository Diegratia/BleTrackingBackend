using System.ComponentModel.DataAnnotations;
using Shared.Contracts;

namespace Data.ViewModels
{
    /// <summary>
    /// DTO for Security actions on Alarm Triggers
    /// Used by security personnel to accept, arrive, and complete investigation
    /// </summary>
    public class AlarmTriggersSecurityActionDto
    {
        /// <summary>
        /// Required for DoneInvestigated action - the investigation result
        /// </summary>
        [Required]
        public InvestigatedResult InvestigationResult { get; set; }

        /// <summary>
        /// Optional free text notes for additional investigation details
        /// </summary>
        [MaxLength(4000)]
        public string? InvestigationNotes { get; set; }
    }
}
