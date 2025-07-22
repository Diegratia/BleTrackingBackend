using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstBuildingDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

    public class MstBuildingCreateDto
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class MstBuildingUpdateDto
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public Guid ApplicationId { get; set; }
    }
}