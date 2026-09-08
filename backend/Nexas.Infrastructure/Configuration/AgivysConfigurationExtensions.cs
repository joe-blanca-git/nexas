using Microsoft.Extensions.Configuration;

namespace Nexas.Infrastructure.Configuration;

public static class AgivysConfigurationExtensions
{
    public static IConfigurationBuilder AddAgivysConfiguration(this IConfigurationBuilder builder, string email, string password)
    {
        return builder.Add(new AgivysConfigurationSource
        {
            Email = email,
            Password = password
        });
    }
}
