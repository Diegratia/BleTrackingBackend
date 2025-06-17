using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class VisitorCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Number { get; set; } = "";
        public string Type { get; set; }
        public string QRCode { get; set; } = "";
        public string mac { get; set; }
        public int CheckinStatus { get; set; }
        public int EnableStatus { get; set; }
        public int Status { get; set; }
        public Guid SiteId { get; set; }
        public int IsMember { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class VisitorCardCreateDto
    {
        public string Name { get; set; } = "";
        public string Number { get; set; } = "";
        public string Type { get; set; }
        public string QRCode { get; set; } = "";
        public string mac { get; set; }
        public Guid SiteId { get; set; }
        public Guid ApplicationId { get; set; }
    }

     public class VisitorCardUpdateDto
    {
        public string Name { get; set; } = "";
        public string Number { get; set; } = "";
        public string Type { get; set; }
        public string QRCode { get; set; } = "";
        public string mac { get; set; }
        public Guid SiteId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}