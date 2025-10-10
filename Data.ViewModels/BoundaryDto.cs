using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class BoundaryDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public string? EngineId { get; set; }
        public BoundaryType BoundaryType { get; set; }
        public int IsActive { get; set; }
        public int Status { get; set; }
    }
    public class BoundaryCreateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public string? EngineId { get; set; }
        public BoundaryType BoundaryType { get; set; }
        public int IsActive { get; set; }
    }
    public class BoundaryUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public string? EngineId { get; set; }
        public BoundaryType BoundaryType { get; set; }
        public int IsActive { get; set; }
    }
}