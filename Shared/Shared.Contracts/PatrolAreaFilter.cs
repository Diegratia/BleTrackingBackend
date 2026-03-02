using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class PatrolAreaFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }

        public JsonElement FloorId { get; set; }
        public JsonElement FloorplanId { get; set; }
        public int? IsActive { get; set; }
        public int? Status { get; set; }
    }
}
