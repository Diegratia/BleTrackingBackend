using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{

    public class MstIntegration
    {
         [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Generate { get; set; } 

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); 

        [Required]
        [ForeignKey("Brand")]  
        public Guid BrandId { get; set; }

        [Required]
        public IntegrationType IntegrationType { get; set; }

        [Required]
        public ApiTypeAuth ApiTypeAuth { get; set; }

        [Required]
        public string ApiUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string ApiAuthUsername { get; set; }

        [Required]
        [StringLength(255)]
        public string ApiAuthPasswd { get; set; }

        [Required]
        [StringLength(255)]
        public string ApiKeyField { get; set; }

        [Required]
        [StringLength(255)]
        public string ApiKeyValue { get; set; }

        [Required]
        [ForeignKey("Application")]  
        public Guid ApplicationId { get; set; }

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public int? Status { get; set; } = 1;

        //relasi dari mstIntegration terhadap domain dibawah ini
        //relasi one to .. terhadap domain dibawah ini
        public virtual MstBrand Brand { get; set; }

        public virtual MstApplication Application { get; set; }
    }

}
