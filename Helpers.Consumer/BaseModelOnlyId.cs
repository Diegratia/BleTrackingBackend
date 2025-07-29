using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers.Consumer
{
    public class BaseModelOnlyId
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
    }
}