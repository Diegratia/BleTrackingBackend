using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Person sessions grouped with incidents
    /// </summary>
    public class PersonSessionsRead
    {
        // =====================================================
        // PERSON INFO
        // =====================================================

        /// <summary>Person ID (VisitorId or MemberId)</summary>
        public Guid? PersonId { get; set; }

        /// <summary>Person name</summary>
        public string? PersonName { get; set; }

        /// <summary>Person type: "Visitor", "Member", "Security"</summary>
        public string? PersonType { get; set; }

        /// <summary>Identity ID (card number, member code, etc.)</summary>
        public string? IdentityId { get; set; }

        /// <summary>Card ID used</summary>
        public Guid? CardId { get; set; }

        /// <summary>Card number</summary>
        public string? CardNumber { get; set; }

        // =====================================================
        // TYPE-SPECIFIC
        // =====================================================

        /// <summary>Visitor ID (if person is visitor)</summary>
        public Guid? VisitorId { get; set; }

        /// <summary>Visitor name</summary>
        public string? VisitorName { get; set; }

        /// <summary>Member ID (if person is member)</summary>
        public Guid? MemberId { get; set; }

        /// <summary>Member name</summary>
        public string? MemberName { get; set; }

        // =====================================================
        // SUMMARY
        // =====================================================

        /// <summary>Total number of sessions</summary>
        public int TotalSessions { get; set; }

        /// <summary>Total duration in minutes</summary>
        public int TotalDurationMinutes { get; set; }

        /// <summary>Total duration formatted</summary>
        public string TotalDurationFormatted { get; set; } = string.Empty;

        /// <summary>Total incidents</summary>
        public int TotalIncidents { get; set; }

        /// <summary>Number of restricted areas visited</summary>
        public int RestrictedAreasVisited { get; set; }

        /// <summary>List of unique areas visited</summary>
        public List<string> AreasVisited { get; set; } = new();

        /// <summary>First area entered</summary>
        public string? FirstAreaEntered { get; set; }

        /// <summary>Last area exited</summary>
        public string? LastAreaExited { get; set; }

        /// <summary>Current area (null if not in any area)</summary>
        public string? CurrentArea { get; set; }

        // =====================================================
        // SESSIONS
        // =====================================================

        /// <summary>List of sessions (minimal, without person info)</summary>
        public List<PersonSessionItemRead> Sessions { get; set; } = new();
    }

    /// <summary>
    /// Response wrapper for grouped sessions - DataTables format
    /// </summary>
    public class GroupedSessionsResponse
    {
        /// <summary>Draw counter from DataTables request (echoed back)</summary>
        public int Draw { get; set; }

        /// <summary>Total records before filtering (recordsTotal)</summary>
        public int RecordsTotal { get; set; }

        /// <summary>Total records after filtering (recordsFiltered)</summary>
        public int RecordsFiltered { get; set; }

        /// <summary>List of persons with their sessions (data)</summary>
        public List<PersonSessionsRead> Persons { get; set; } = new();

        /// <summary>Summary statistics (optional)</summary>
        public VisitorSessionSummaryRead? Summary { get; set; }

        /// <summary>Visual paths (optional)</summary>
        public VisualPathsRead? VisualPaths { get; set; }
    }
}
