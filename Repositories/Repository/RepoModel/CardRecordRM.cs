using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class CardUsageHistory
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public string? CardNumber { get; set; }
    }
}