    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;
    using Helpers.Consumer;

    namespace Entities.Models
    {
    public class CardAccess : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Required]
        [Column("access_number")]
        public string? AccessNumber { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;
        public ICollection<CardAccessMaskedArea?> CardAccessMaskedAreas { get; set; } = new List<CardAccessMaskedArea?>();
        public MstApplication? Application { get; set; }
    }
}
        