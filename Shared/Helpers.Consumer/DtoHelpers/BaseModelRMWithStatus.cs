using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helpers.Consumer.DtoHelpers
{
    public class BaseModelWithStatusRM
    {
        [JsonPropertyOrder(-10)]
        public Guid Id { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }
        public Guid ApplicationId { get; set; }
    }
}