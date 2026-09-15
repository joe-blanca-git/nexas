using Microsoft.AspNetCore.Identity;

namespace Nexas.SystemManager.Domain.Entities
{
    /// <summary>
    /// Usuário de Identity do Nexas. Estende o IdentityUser padrão só com o nome completo —
    /// telefone já é coberto pelo PhoneNumber nativo do IdentityUser.
    /// </summary>
    public class NexasUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
