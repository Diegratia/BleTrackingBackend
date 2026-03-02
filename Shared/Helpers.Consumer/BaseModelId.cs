using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helpers.Consumer
{
    public class BaseModelId
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid(); 
    }

    
}