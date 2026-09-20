using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nexas.SystemManager.Domain.Entities
{
    [Index(nameof(ApiKey), IsUnique = true)]
    public class SystemApp
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? UrlLogo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? UrlDomain { get; set; }

        /// <summary>Status da aplicação: "active", "inactive" ou "pending".</summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Chave de API única da aplicação (prefixo "nxs_" + 48 chars hex, 24 bytes de entropia).
        /// Gerada automaticamente na criação; pode ser regenerada via POST {id}/api-key/regenerate,
        /// o que invalida a anterior. Guardada em texto puro por simplicidade (sem hashing) —
        /// suficiente para o estágio atual, mas não é o ideal para um cofre de segredos de produção.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Client ID OAuth do Google configurado pelo dono da aplicação (não é segredo — é
        /// público, usado no botão "Entrar com Google" do frontend da própria aplicação). Quando
        /// nulo, o login com Google fica desabilitado para os usuários finais dessa aplicação.
        /// O dono precisa ter criado esse Client ID no Google Cloud dele e autorizado o próprio
        /// domínio da aplicação como origem — a Nexas só valida a audience do token recebido
        /// contra esse valor, nunca chama a API do Google com credenciais próprias.
        /// </summary>
        [MaxLength(255)]
        public string? GoogleClientId { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedUser { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedUser { get; set; }
    }
}
