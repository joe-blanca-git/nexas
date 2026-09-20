using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexas.SystemManager.Domain.Entities
{
    public class UserLoginHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = null!;

        [Required]
        public DateTime LoginDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual NexasUser User { get; set; } = null!;
    }
}
