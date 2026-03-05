using System;

namespace Shared.Contracts.Read
{
    public class PatrolShiftReplacementRead : BaseRead
    {
        public Guid? PatrolAssignmentId { get; set; }
        public SecurityListRead? OriginalSecurity { get; set; }
        public SecurityListRead? SubstituteSecurity { get; set; }
        public DateOnly? ReplacementStartDate { get; set; }
        public DateOnly? ReplacementEndDate { get; set; }
        public string? Reason { get; set; }
    }
}
