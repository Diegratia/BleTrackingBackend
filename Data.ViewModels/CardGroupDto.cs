using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
   public class CardGroupCreateDto : CardCreateDto
{
    public string? Name { get; set; }
    public string? Remarks { get; set; }
    public Guid ApplicationId { get; set; }
    public List<Guid?> CardAccessIds { get; set; } = new();
    public List<Guid?> CardIds { get; set; } = new();
}

    public class CardGroupUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public List<Guid?> CardAccessIds { get; set; } = new();
        public List<Guid?> CardIds { get; set; } = new();
    }

    public class CardGroupDto : BaseModelDto
  {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Remarks { get; set; }
        public List<CardMinimalDto?> Cards { get; set; } = new();
        public List<CardAccessDto?> CardAccesses { get; set; } = new();
    }

}