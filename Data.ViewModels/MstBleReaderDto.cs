using System;

namespace Data.ViewModels
{
    public class MstBleReaderDto : BaseModelDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string? Name { get; set; }
        public string? Ip { get; set; }
        public string? Gmac { get; set; }
        public string EngineReaderId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstBrandDto Brand { get; set; }
    }

    public class MstBleReaderCreateDto : BaseModelDto
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Gmac { get; set; }
        public string EngineReaderId { get; set; }
    }

    public class MstBleReaderUpdateDto : BaseModelDto
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Gmac { get; set; }
        public string EngineReaderId { get; set; }
    }

}