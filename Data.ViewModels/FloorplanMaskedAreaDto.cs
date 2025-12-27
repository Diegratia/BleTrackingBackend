using System;
using System.Text.Json.Serialization;

namespace Data.ViewModels
{
    public class FloorplanMaskedAreaDto : BaseModelDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public Guid FloorplanId { get; set;}
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        public string AreaShape { get; set; }
        public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        // public string EngineAreaId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstFloorDto Floor { get; set; }
        public MstFloorplanDto Floorplan { get; set; }
    }
    

    public class FloorplanMaskedAreaMinimalDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    
     public class OpenFloorplanMaskedAreaDto : BaseModelDto
    {
        public int Generate { get; set; }

        [JsonPropertyName("floorplan_masked_area_id")]
        public Guid Id { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        public string AreaShape { get; set; }
        public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        // public string EngineAreaId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        // public MstFloorDto Floor { get; set; }
        // public MstFloorplanDto Floorplan { get; set; }
    }

     public class FloorplanMaskedAreaCreateDto : BaseModelDto
    {
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        public string AreaShape { get; set; }
        public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        // public string EngineAreaId { get; set; }
        // public long WideArea { get; set; }
        // public long PositionPxX { get; set; }
        // public long PositionPxY { get; set; }
    }

     public class FloorplanMaskedAreaUpdateDto 
    {
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string Name { get; set; }   
        public string AreaShape { get; set; }
        public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        // public string EngineAreaId { get; set; }
        // public long WideArea { get; set; }
        // public long PositionPxX { get; set; }
        // public long PositionPxY { get; set; }
    }

    //     public enum RestrictedStatus
    // {
    //     Restrict,
    //     NonRestrict 
    // }
}