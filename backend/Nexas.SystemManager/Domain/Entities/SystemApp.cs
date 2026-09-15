using System;

namespace Nexas.SystemManager.Domain.Entities
{
    public class SystemApp
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? UrlLogo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? UrlDomain { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedUser { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedUser { get; set; }
    }
}
