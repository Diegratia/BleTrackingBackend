using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class GeofenceDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public int Status { get; set; }
    }
    public class GeofenceCreateDto
    {
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
    }
    public class GeofenceUpdateDto
    {
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
    }
}