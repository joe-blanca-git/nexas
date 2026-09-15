using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nexas.SystemManager.Filters
{
    /// <summary>
    /// Os endpoints de /api/v1/auth (login, register, refresh, etc.) vêm prontos do
    /// MapIdentityApi do ASP.NET Core, então não têm controller/action pra anotar com
    /// [SwaggerOperation]. Este filtro documenta cada um manualmente, por rota + verbo.
    /// </summary>
    public class AuthEndpointsDocumentationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

            if (!path.Contains("api/v1/auth"))
                return;

            if (path.EndsWith("register") && method == "POST")
            {
                operation.Summary = "Registra um novo usuário";
                operation.Description = "Cria uma nova conta com nome, e-mail, telefone e senha (endpoint próprio do Nexas, não o /register padrão do Identity). " +
                    "Todos os campos são obrigatórios. O e-mail é o identificador único da conta: se já existir um usuário cadastrado com o mesmo e-mail, " +
                    "a requisição é rejeitada com 400. O primeiro usuário criado no sistema recebe automaticamente as roles Admin+Dev; os demais recebem Owner.";

                AddResponse(operation, "200", "Usuário criado com sucesso. A resposta não tem corpo.", new { });
                AddResponse(operation, "400", "Campo obrigatório ausente, e-mail já cadastrado ou senha fora dos requisitos mínimos.", new
                {
                    statusCode = 400,
                    message = "Um ou mais erros de validação ocorreram na sua requisição.",
                    errors = new object[] { new { propertyName = "Email", errorMessage = "Este e-mail já está cadastrado." } },
                    stackTrace = (string?)null
                });
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("login") && method == "POST")
            {
                operation.Summary = "Autentica um usuário";
                operation.Description = "Valida e-mail e senha e retorna um par de tokens (accessToken/refreshToken) para uso nas rotas autenticadas.";

                // useCookies/useSessionCookies só fazem sentido pra login baseado em cookie (Razor Pages/Blazor
                // Server). Essa API é 100% Bearer token, então esses parâmetros nunca devem ser usados aqui.
                operation.Parameters = operation.Parameters
                    ?.Where(p => p.Name != "useCookies" && p.Name != "useSessionCookies")
                    .ToList();

                AddResponse(operation, "200", "Login efetuado com sucesso.", new
                {
                    tokenType = "Bearer",
                    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    expiresIn = 3600,
                    refreshToken = "CfDJ8...=="
                });
                AddResponse(operation, "401", "E-mail ou senha inválidos, ou conta bloqueada/não confirmada.", new
                {
                    statusCode = 401,
                    message = "E-mail ou senha inválidos.",
                    errors = (object?)null,
                    stackTrace = (string?)null
                });
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("refresh") && method == "POST")
            {
                operation.Summary = "Renova o token de acesso";
                operation.Description = "Troca um refreshToken válido por um novo par accessToken/refreshToken, sem exigir login novamente.";

                AddResponse(operation, "200", "Novo par de tokens gerado com sucesso.", new
                {
                    tokenType = "Bearer",
                    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    expiresIn = 3600,
                    refreshToken = "CfDJ8...=="
                });
                AddResponse(operation, "401", "Refresh token inválido, expirado ou já utilizado.", null);
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("resendconfirmationemail") && method == "POST")
            {
                operation.Summary = "Reenvia o e-mail de confirmação de conta";
                operation.Description = "Reenvia o link de confirmação para o e-mail informado. Por segurança, sempre retorna 200, " +
                    "mesmo que o e-mail não esteja cadastrado (evita expor quais e-mails existem na base).";

                AddResponse(operation, "200", "Requisição aceita (o e-mail é enviado se a conta existir).", new { });
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("confirmemail") && method == "GET")
            {
                operation.Summary = "Confirma o e-mail de uma conta";
                operation.Description = "Valida o token de confirmação enviado por e-mail (parâmetros userId e code) e marca a conta como confirmada.";

                AddResponse(operation, "200", "E-mail confirmado com sucesso.", null, "text/plain");
                AddResponse(operation, "401", "Token de confirmação inválido, expirado ou usuário inexistente.", null);
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("forgotpassword") && method == "POST")
            {
                operation.Summary = "Solicita redefinição de senha";
                operation.Description = "Envia um e-mail com o token de redefinição de senha. Por segurança, sempre retorna 200, " +
                    "mesmo que o e-mail não esteja cadastrado.";

                AddResponse(operation, "200", "Requisição aceita (o e-mail é enviado se a conta existir).", new { });
                AddResponse(operation, "400", "E-mail com formato inválido.", null);
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("resetpassword") && method == "POST")
            {
                operation.Summary = "Redefine a senha usando o token recebido por e-mail";
                operation.Description = "Define uma nova senha a partir do token gerado por /forgotPassword.";

                AddResponse(operation, "200", "Senha redefinida com sucesso.", new { });
                AddResponse(operation, "400", "Token inválido/expirado ou nova senha fora dos requisitos mínimos.", new
                {
                    statusCode = 400,
                    message = "Um ou mais erros de validação ocorreram na sua requisição.",
                    errors = new object[] { new { propertyName = "Password", errorMessage = "A senha informada é muito curta." } },
                    stackTrace = (string?)null
                });
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("manage/info") && method == "GET")
            {
                operation.Summary = "Obtém os dados da conta autenticada";
                operation.Description = "Retorna e-mail e status de confirmação do usuário identificado pelo token enviado no header Authorization.";

                AddResponse(operation, "200", "Dados da conta retornados com sucesso.", new
                {
                    email = "usuario@nexas.com.br",
                    isEmailConfirmed = true
                });
                AddResponse(operation, "401", "Token ausente, inválido ou expirado.", null);
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
                return;
            }

            if (path.EndsWith("manage/info") && method == "POST")
            {
                operation.Summary = "Atualiza e-mail e/ou senha da conta autenticada";
                operation.Description = "Permite trocar o e-mail (newEmail) e/ou a senha (oldPassword + newPassword) do usuário identificado pelo token.";

                AddResponse(operation, "200", "Dados atualizados com sucesso.", new
                {
                    email = "novo-email@nexas.com.br",
                    isEmailConfirmed = false
                });
                AddResponse(operation, "400", "Senha atual incorreta, novo e-mail já cadastrado ou nova senha fora dos requisitos mínimos.", new
                {
                    statusCode = 400,
                    message = "Um ou mais erros de validação ocorreram na sua requisição.",
                    errors = new object[] { new { propertyName = "Password", errorMessage = "A senha atual informada está incorreta." } },
                    stackTrace = (string?)null
                });
                AddResponse(operation, "401", "Token ausente, inválido ou expirado.", null);
                AddResponse(operation, "500", "Erro inesperado no servidor.", null);
            }
        }

        private static void AddResponse(OpenApiOperation operation, string statusCode, string description, object? example, string contentType = "application/json")
        {
            var response = new OpenApiResponse { Description = description };

            if (example != null)
            {
                response.Content[contentType] = new OpenApiMediaType { Example = ToOpenApiAny(example) };
            }

            operation.Responses[statusCode] = response;
        }

        private static IOpenApiAny ToOpenApiAny(object? value)
        {
            if (value is null) return new OpenApiNull();
            return ToOpenApiAny(JsonSerializer.SerializeToElement(value));
        }

        private static IOpenApiAny ToOpenApiAny(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new OpenApiObject();
                    foreach (var prop in element.EnumerateObject())
                        obj[prop.Name] = ToOpenApiAny(prop.Value);
                    return obj;
                case JsonValueKind.Array:
                    var arr = new OpenApiArray();
                    foreach (var item in element.EnumerateArray())
                        arr.Add(ToOpenApiAny(item));
                    return arr;
                case JsonValueKind.String:
                    return new OpenApiString(element.GetString());
                case JsonValueKind.Number:
                    return element.TryGetInt64(out var l) ? new OpenApiLong(l) : new OpenApiDouble(element.GetDouble());
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return new OpenApiBoolean(element.GetBoolean());
                default:
                    return new OpenApiNull();
            }
        }
    }
}
