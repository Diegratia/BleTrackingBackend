using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class Geofence : BaseModelWithTime, IApplicationEntity
    {

        [Column("name")]
        public string? Name { get; set; }

        [Column("area_shape")]
        public string? AreaShape { get; set; }
        [Column("color")]
        public string? Color { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        public MstApplication Application { get; set; }

    }
}