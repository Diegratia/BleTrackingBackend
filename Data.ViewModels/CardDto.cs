using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class CardDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? CardBarcode { get; set; }
        public bool? IsMultiSite { get; set; }
        public Guid? RegisteredSite { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public bool? StatusCard { get; set; }
    }

    public class CardCreateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? CardBarcode { get; set; }
        public bool? IsMultiSite { get; set; }
        public Guid? RegisteredSite { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
    }
    
     public class CardUpdateDto
{
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? CardBarcode { get; set; }
        public bool? IsMultiSite { get; set; }
        public Guid? RegisteredSite { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
    }
}


