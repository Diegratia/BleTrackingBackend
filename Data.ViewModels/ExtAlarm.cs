using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
   public class EventSubscriptionRequest
    {
        public int[] EventTypes { get; set; }
        public string EventDest { get; set; }
    }
}
