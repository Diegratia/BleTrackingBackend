using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public enum StatusActive
    {
        NonActive,
        Active
        
    }

    public class User : BaseModel, IApplicationEntity
    {
        [Required]
        [Column("username")]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; } // Hash

        [Required]
        [Column("is_created_password")]
        public int IsCreatedPassword { get; set; }

        [Required]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("is_email_confirmation")]
        public int IsEmailConfirmation { get; set; }

        [Required]
        [Column("email_confirmation_code")]
        [StringLength(255)]
        public string EmailConfirmationCode { get; set; }

        [Required]
        [Column("email_confirmation_expired_at")]
        public DateTime EmailConfirmationExpiredAt { get; set; }

        [Required]
        [Column("email_confirmation_at")]
        public DateTime EmailConfirmationAt { get; set; }

        [Required]
        [Column("last_login_at")]
        public DateTime LastLoginAt { get; set; }

        [Required]
        [Column("status_active")]
        public StatusActive StatusActive { get; set; }

        [Required]
        [Column("group_id")]
        public Guid GroupId { get; set; } // Foreign key

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; } // Foreign key

        [ForeignKey("GroupId")]
        public UserGroup Group { get; set; } // Navigation property

        [ForeignKey("ApplicationId")]
        public MstApplication Application { get; set; } // Navigation property
    }
}