using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class OverpopulatingDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public string? EngineId { get; set; }
        public int? MaxCapacity { get; set; }
        public int IsActive { get; set; }
        public int Status { get; set; }
    }
    public class OverpopulatingCreateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int? MaxCapacity { get; set; }
        public string? EngineId { get; set; }
        public int IsActive { get; set; }
    }
    public class OverpopulatingUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int? MaxCapacity { get; set; }
        public string? EngineId { get; set; }
        public int IsActive { get; set; }
    }
}