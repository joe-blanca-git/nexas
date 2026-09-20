using System;
using System.Collections.Generic;

namespace Nexas.SystemManager.Features.Applications.DTOs
{
    public class AppEndUserSummaryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AppRoleDto> Roles { get; set; } = new();
    }
}
