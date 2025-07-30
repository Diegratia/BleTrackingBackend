
using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstFloorDto : BaseModelDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
        public string? FloorImage { get; set; }
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public float MeterPerPx { get; set; }
        public long EngineFloorId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstBuildingDto Building { get; set; }
    }

    public class MstFloorCreateDto : BaseModelDto
    {
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
        public IFormFile? FloorImage { get; set; }
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public float MeterPerPx { get; set; }
        public long EngineFloorId { get; set; }
    }

    public class MstFloorUpdateDto : BaseModelDto
    {
        public Guid? BuildingId { get; set; }
        public string? Name { get; set; }
        public IFormFile? FloorImage { get; set; }
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public float MeterPerPx { get; set; }
        public long EngineFloorId { get; set; }
    }
    public class MstFloorImportDto
    {
        public string BuildingId { get; set; } 
        public string? Name { get; set; }
        public string? FloorImage { get; set; } = "";
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public float MeterPerPx { get; set; }
        public long EngineFloorId { get; set; }
        public string CreatedBy { get; set; }
        public int? Status { get; set; } = 1;
    }

    public class ImportResultDto
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        public int ProcessedRows { get; set; }
        public int SuccessfulRows { get; set; }
        public int Code { get; set; }
}
    }
        