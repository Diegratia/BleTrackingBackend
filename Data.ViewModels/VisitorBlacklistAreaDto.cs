using System;
using System.Text.Json.Serialization;

namespace Data.ViewModels
{
    public class VisitorBlacklistAreaDto : BaseModelDto
    {
        public long Generate { get; set; } 

        public Guid Id { get; set; }

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid VisitorId { get; set; }

        public FloorplanMaskedAreaDto FloorplanMaskedArea { get; set; }
        public VisitorDto Visitor { get; set; }

    }
    
      public class OpenVisitorBlacklistAreaDto : BaseModelDto
    {
        public long Generate { get; set; } 

        [JsonPropertyName("visitor_blacklist_area_id")]
        public Guid Id { get; set; }

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid VisitorId { get; set; }

        // public FloorplanMaskedAreaDto FloorplanMaskedArea { get; set; }
        // public VisitorDto Visitor { get; set; }

    }

    public class VisitorBlacklistAreaCreateDto : BaseModelDto
    {

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid VisitorId { get; set; }

    }

    public class VisitorBlacklistAreaUpdateDto 
    {

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid VisitorId { get; set; }

    }
}
