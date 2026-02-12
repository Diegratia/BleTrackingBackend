using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class PatrolCaseFilter : BaseFilter
    {
        // BaseFilter already provides: Search, Page, PageSize, SortColumn, SortDir, DateFrom, DateTo

        public CaseType? CaseType { get; set; }
        public CaseStatus? CaseStatus { get; set; }

        // Use JsonElement for ID filters to support both single Guid and Guid array
        public JsonElement SecurityId { get; set; }
        public JsonElement PatrolAssignmentId { get; set; }
        public JsonElement PatrolRouteId { get; set; }
    }
}
