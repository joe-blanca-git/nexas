using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nexas.SystemManager.Domain.Entities
{
    /// <summary>
    /// Usuário final de uma Application cadastrada na Nexas — não confundir com NexasUser,
    /// que é quem administra o Portal Nexas (Owner/Admin). Cada AppEndUser pertence a
    /// exatamente uma Application (ApplicationId): o e-mail só precisa ser único DENTRO da
    /// aplicação (índice composto ApplicationId+NormalizedEmail abaixo), então a mesma pessoa
    /// pode se cadastrar em duas aplicações diferentes com contas completamente independentes
    /// (senha, nome, tudo).
    /// </summary>
    [Index(nameof(ApplicationId), nameof(NormalizedEmail), IsUnique = true)]
    public class AppEndUser
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicationId { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual SystemApp Application { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string NormalizedEmail { get; set; } = null!;

        [MaxLength(256)]
        public string? FullName { get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;

        [MaxLength(64)]
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<AppEndUserRole> UserRoles { get; set; } = new List<AppEndUserRole>();
    }
}
