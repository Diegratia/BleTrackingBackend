using System;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace Data.ViewModels
{

    public class MstOrganizationDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public int Generate { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? OrganizationHost { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

     public class OpenMstOrganizationDto : BaseModelDto
    {
        [JsonPropertyName("organization_id")]       public Guid Id { get; set; }
        public int Generate { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? OrganizationHost { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

    public class MstOrganizationCreateDto : BaseModelDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? OrganizationHost { get; set; }
    }

    public class MstOrganizationUpdateDto 
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? OrganizationHost { get; set; }
    }

}