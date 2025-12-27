
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstFloorDto : BaseModelDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstBuildingDto Building { get; set; }
    }

    public class OpenMstFloorDto : BaseModelDto
    {
        public int Generate { get; set; }
        [JsonPropertyName("floor_id")]
        public Guid Id { get; set; }
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

    public class MstFloorCreateDto : BaseModelDto
    {
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
    }

    public class MstFloorUpdateDto 
    {
        public Guid? BuildingId { get; set; }
        public string? Name { get; set; }
        public Guid? ApplicationId { get; set; }
    }
    public class MstFloorImportDto
    {
        public string BuildingId { get; set; } 
        public string? Name { get; set; }
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
        