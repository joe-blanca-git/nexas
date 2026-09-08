using Microsoft.Extensions.Configuration;

namespace Nexas.Infrastructure.Configuration;

public class AgivysConfigurationSource : IConfigurationSource
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string LoginUrl { get; set; } = "http://agivys-api-container:5000/api/v1/authentication/login";
    public string IntegrationsUrl { get; set; } = "http://agivys-api-container:5000/api/v1/integration";

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AgivysConfigurationProvider(this);
    }
}
