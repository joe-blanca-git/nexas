using System;

namespace Nexas.SystemManager.Features.Applications.DTOs
{
    public class AppRoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAppRoleDto
    {
        public string Name { get; set; } = null!;
    }
}
