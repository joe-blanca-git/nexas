using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexas.SystemManager.Domain.Entities
{
    /// <summary>
    /// Associação many-to-many entre AppEndUser e AppRole — um usuário final pode acumular
    /// vários papéis ao mesmo tempo (ex.: "Professor" e "Admin" na mesma Application). Chave
    /// composta configurada via Fluent API em SystemManagerDbContext.OnModelCreating (Data
    /// Annotations não suportam chave composta).
    /// </summary>
    public class AppEndUserRole
    {
        public Guid AppEndUserId { get; set; }

        [ForeignKey(nameof(AppEndUserId))]
        public virtual AppEndUser AppEndUser { get; set; } = null!;

        public Guid AppRoleId { get; set; }

        [ForeignKey(nameof(AppRoleId))]
        public virtual AppRole AppRole { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
