using Microsoft.AspNetCore.Identity.Data;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nexas.SystemManager.Filters
{
    /// <summary>
    /// O corpo do /login (LoginRequest) é fixo pelo framework e sempre inclui twoFactorCode/
    /// twoFactorRecoveryCode, mesmo sem 2FA habilitado. Como o /manage/2fa já está escondido
    /// do Swagger, esses dois campos só confundiriam quem for chamar o endpoint.
    /// </summary>
    public class HideLoginTwoFactorFieldsFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(LoginRequest))
                return;

            schema.Properties.Remove("twoFactorCode");
            schema.Properties.Remove("twoFactorRecoveryCode");
        }
    }
}
