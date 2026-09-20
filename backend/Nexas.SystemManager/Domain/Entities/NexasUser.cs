using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexas.SystemManager.Domain.Entities
{
    /// <summary>
    /// Usuário de Identity do Nexas. Estende o IdentityUser padrão só com o nome completo —
    /// telefone já é coberto pelo PhoneNumber nativo do IdentityUser.
    /// </summary>
    public class NexasUser : IdentityUser
    {
        public string? FullName { get; set; }

        /// <summary>
        /// Avatar do usuário como data URL (ex.: "data:image/jpeg;base64,...."). O frontend já
        /// redimensiona/comprime a imagem para um quadrado pequeno (ver AVATAR_SIZE em
        /// ProfileTabComponent) antes de enviar, então isso nunca deveria passar de ~50-100KB —
        /// mesmo assim usamos MEDIUMTEXT para não correr risco de truncar em fotos maiores.
        /// </summary>
        [Column(TypeName = "MEDIUMTEXT")]
        public string? AvatarBase64 { get; set; }

        /// <summary>Preferências de privacidade/comunicação, editáveis na aba Privacidade de "Minha Conta".</summary>
        public bool ReceiveMarketingEmails { get; set; } = true;

        public bool ReceiveProductNotifications { get; set; } = true;
    }
}
