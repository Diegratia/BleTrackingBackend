using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class UserFilter : BaseFilter
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public JsonElement GroupId { get; set; }  // Supports both single and array
        public bool? IsEmailConfirmed { get; set; }
        public int? StatusActive { get; set; }
        public bool? IsIntegration { get; set; }
    }
}
