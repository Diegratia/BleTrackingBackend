using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolShiftReplacementCreateDto
    {
        public Guid PatrolAssignmentId { get; set; }
        public Guid OriginalSecurityId { get; set; }
        public Guid SubstituteSecurityId { get; set; }
        public DateOnly ReplacementStartDate { get; set; }
        public DateOnly ReplacementEndDate { get; set; }
        public string? Reason { get; set; }
    }

    public class PatrolShiftReplacementUpdateDto
    {
        public Guid? OriginalSecurityId { get; set; }
        public Guid? SubstituteSecurityId { get; set; }
        public DateOnly? ReplacementStartDate { get; set; }
        public DateOnly? ReplacementEndDate { get; set; }
        public string? Reason { get; set; }
    }
}
