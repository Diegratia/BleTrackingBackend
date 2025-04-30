using System;

namespace Data.ViewModels
{

    public class MstOrganizationDto
    {
        public Guid Id { get; set; }
        public int Generate { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string OrganizationHost { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

    public class MstOrganizationCreateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string OrganizationHost { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class MstOrganizationUpdateDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string OrganizationHost { get; set; }
        public Guid ApplicationId { get; set; }
    }

}