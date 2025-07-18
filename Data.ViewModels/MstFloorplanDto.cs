using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstFloorplanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid FloorId { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }

        public MstFloorDto Floor { get; set; }
    }

    public class MstFloorplanCreateDto
    {
        public string Name { get; set; }
        public Guid FloorId { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class MstFloorplanUpdateDto
    {
        public string Name { get; set; }
        public Guid FloorId { get; set; }
        public Guid ApplicationId { get; set; }
    }

    
}