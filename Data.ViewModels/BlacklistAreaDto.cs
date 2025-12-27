using System;
using System.Text.Json.Serialization;

namespace Data.ViewModels
{
    public class BlacklistAreaDto : BaseModelDto
    {
        // public long Generate { get; set; }

        public Guid? Id { get; set; }

        public Guid? FloorplanMaskedAreaId { get; set; }

        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }

        public FloorplanMaskedAreaDto? FloorplanMaskedArea { get; set; }
        public VisitorDto? Visitor { get; set; }
        public MstMemberDto? Member { get; set; }

    }

    public class OpenBlacklistAreaDto : BaseModelDto
    {
        public long Generate { get; set; }

        [JsonPropertyName("visitor_blacklist_area_id")]
        public Guid Id { get; set; }

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }


        // public FloorplanMaskedAreaDto FloorplanMaskedArea { get; set; }
        // public VisitorDto Visitor { get; set; }

    }

    public class BlacklistAreaCreateDto : BaseModelDto
    {

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }

    }

    public class BlacklistAreaUpdateDto
    {

        public Guid FloorplanMaskedAreaId { get; set; }

        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }

    }

    public class BlacklistAreaRequestDto : BaseModelDto
    {
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public List<BlacklistAreaCreateDto> BlacklistAreas { get; set; }
    }

            public class BlacklistSummaryDto
    {
            public Guid Id { get; set; }
            public String? BlacklistPersonName { get; set; }
    }
    
}
