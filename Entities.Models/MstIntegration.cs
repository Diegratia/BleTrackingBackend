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
        [Column("_generate")]
        public int Generate { get; set; } 

        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid(); 

        [Required]
        [ForeignKey("Brand")]  
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Required]
        [Column("integration_type")]
        public IntegrationType IntegrationType { get; set; }

        [Required]
        [Column("api_type_auth")]
        public ApiTypeAuth ApiTypeAuth { get; set; }

        [Required]
        [Column("api_url")]
        public string ApiUrl { get; set; }

        [Required]
        [StringLength(255)]
        [Column("api_auth_username")]
        public string ApiAuthUsername { get; set; }

        [Required]
        [StringLength(255)]
        [Column("api_auth_passwd")]
        public string ApiAuthPasswd { get; set; }

        [Required]
        [StringLength(255)]
        [Column("api_key_field")]
        public string ApiKeyField { get; set; }

        [Required]
        [StringLength(255)]
        [Column("api_key_value")]
        public string ApiKeyValue { get; set; }

        [Required]
        [ForeignKey("Application")]  
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        //relasi dari mstIntegration terhadap domain dibawah ini
        //relasi one to .. terhadap domain dibawah ini
        public virtual MstBrand Brand { get; set; }

        public virtual MstApplication Application { get; set; }
    }

}