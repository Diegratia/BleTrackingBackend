using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolAreaDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int IsActive { get; set; }
        public int Status { get; set; }
    }
    public class PatrolAreaCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int IsActive { get; set; }
    }
    public class PatrolAreaUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int IsActive { get; set; }
    }

     public class PatrolAreaLookUpDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? FloorName { get; set; }
        public string? FloorplanName { get; set; }
        public int? IsActive { get; set; }
    }
}