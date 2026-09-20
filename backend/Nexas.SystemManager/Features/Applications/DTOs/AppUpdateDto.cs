using System;

namespace Nexas.SystemManager.Features.Applications.DTOs
{
    public class AppUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? UrlLogo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? UrlDomain { get; set; }
        public string? Status { get; set; }
        public string? GoogleClientId { get; set; }
    }
}
