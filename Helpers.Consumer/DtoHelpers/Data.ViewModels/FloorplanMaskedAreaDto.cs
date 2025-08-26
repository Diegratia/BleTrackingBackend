using System;

namespace Data.ViewModels
{
    public class FloorplanMaskedAreaDtoz : BaseModelDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public Guid FloorplanId { get; set;}
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        // public string AreaShape { get; set; }
        // public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        // public string EngineAreaId { get; set; }
        // public string CreatedBy { get; set; }
        // public DateTime CreatedAt { get; set; }
        // public string UpdatedBy { get; set; }
        // public DateTime UpdatedAt { get; set; }
        // public int? Status { get; set; }
        // public MstFloorDto Floor { get; set; }
        // public MstFloorplanDto Floorplan { get; set; }
    }

}