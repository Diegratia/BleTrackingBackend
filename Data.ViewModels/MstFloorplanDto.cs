using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstFloorplanDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public long Generate { get; set; }
        public string? Name { get; set; }
        public Guid FloorId { get; set; }
        // public Guid ApplicationId { get; set; }
        public string? FloorplanImage { get; set; }
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public long? EngineId { get; set; } 
        public float MeterPerPx { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }
        public int MaskedAreaCount { get; set; }
        public int DeviceCount { get; set; }
        public MstFloorDto Floor { get; set; }
    }

       public class OpenMstFloorplanDto : BaseModelDto
    {
        public long Generate { get; set; }

         [JsonPropertyName("floorplan_id")]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid FloorId { get; set; }
        // public Guid ApplicationId { get; set; }
        
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }
        public int MaskedAreaCount { get; set; }
        public int DeviceCount { get; set; }
        // public MstFloorDto Floor { get; set; }
    }

    public class MstFloorplanCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public Guid FloorId { get; set; }
        public IFormFile? FloorplanImage { get; set; }

        public float PixelX { get; set; }

        public float PixelY { get; set; }

        public float FloorX { get; set; }

        public float FloorY { get; set; }

        public float MeterPerPx { get; set; }
        public long? EngineId { get; set; } 
        // public Guid ApplicationId { get; set; }
    }

    public class MstFloorplanUpdateDto
    {
        public string? Name { get; set; }
        public Guid FloorId { get; set; }
        public IFormFile? FloorplanImage { get; set; }

        public float PixelX { get; set; }

        public float PixelY { get; set; }

        public float FloorX { get; set; }

        public float FloorY { get; set; }

        public float MeterPerPx { get; set; }
        public long? EngineId { get; set; } 
        // public Guid ApplicationId { get; set; }
    }

    
}