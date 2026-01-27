using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class TrackingTransaction : BaseModelId, IApplicationEntity
    {

        [Column("trans_time")]
        public DateTime? TransTime { get; set; }

        [Column("reader_id")]
        public Guid? ReaderId { get; set; } 

        [Column("card_id")]
        public Guid? CardId { get; set; }

        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }
        [Column("member_id")]
        public Guid? MemberId { get; set; }

        [Column("floorplan_masked_area_id")]
        public Guid? FloorplanMaskedAreaId { get; set; }

        [Column("coordinate_x")]
        public float? CoordinateX { get; set; }

        [Column("coordinate_y")]
        public float? CoordinateY { get; set; }

        [Column("coordinate_px_x")]
        public float? CoordinatePxX { get; set; }

        [Column("coordinate_px_y")]
        public float? CoordinatePxY { get; set; }

        [Column("alarm_status")]
        public AlarmStatus? AlarmStatus { get; set; }

        [Column("battery")]
        public long? Battery { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstApplication Application { get; set; }
        public MstBleReader Reader { get; set; }
        public MstMember Member { get; set; }
        public Visitor Visitor { get; set; }
        public Card Card { get; set; }
        public FloorplanMaskedArea FloorplanMaskedArea { get; set; }
    }
}