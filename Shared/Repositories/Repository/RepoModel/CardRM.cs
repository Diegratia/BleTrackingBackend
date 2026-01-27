using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class CardRM
    {

    }

    public class CardDashboardRM
    {
        public Guid Id { get; set; }
        public string Dmac { get; set; }
        public string CardNumber { get; set; }
    }
}