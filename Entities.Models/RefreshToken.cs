using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
public class RefreshToken
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("User")]
        public virtual User User { get; set; }

    }
}
