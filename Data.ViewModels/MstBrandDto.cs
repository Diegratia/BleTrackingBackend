using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Data.ViewModels

{
    public class MstBrandDto
    {
        public int Generate { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public int? Status { get; set; }
    }

    public class MstBrandCreateDto
    {
        public string Name { get; set; }
        public string Tag { get; set; }
    }

    public class MstBrandUpdateDto
    {
        public string Name { get; set; }
        public string Tag { get; set; }
    }
}