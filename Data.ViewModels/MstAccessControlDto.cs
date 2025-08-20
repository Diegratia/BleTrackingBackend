namespace Data.ViewModels
{
    public class MstAccessControlDto : BaseModelDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Channel { get; set; }
        public string DoorId { get; set; }
        public string? Raw { get; set; }
        public Guid? IntegrationId { get; set; }
        // public Guid ApplicationId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstBrandDto Brand { get; set; }
        public MstIntegrationDto? Integration { get; set; }
    }

    public class MstAccessControlCreateDto : BaseModelDto
    {
        public Guid BrandId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Channel { get; set; }
        public string DoorId { get; set; }
        public string? Raw { get; set; }
        public Guid? IntegrationId { get; set; }
        // public Guid ApplicationId { get; set; }
    }

    public class MstAccessControlUpdateDto
    {
        public Guid BrandId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Channel { get; set; }
        public string DoorId { get; set; }
        public string? Raw { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}