using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Nexas.SystemManager.Domain.Entities
{
    /// <summary>
    /// Papel (role) definido pelo dono de uma Application para os seus próprios usuários finais
    /// (ex.: "Admin", "Professor", "Aluno") — sem enum fixo, o dono cria/remove livremente.
    /// Escopado por ApplicationId (nome único só DENTRO da aplicação — duas aplicações podem
    /// ter cada uma o seu próprio papel "Admin", são entidades independentes).
    /// </summary>
    [Index(nameof(ApplicationId), nameof(Name), IsUnique = true)]
    public class AppRole
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicationId { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual SystemApp Application { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<AppEndUserRole> UserRoles { get; set; } = new List<AppEndUserRole>();
    }
}
