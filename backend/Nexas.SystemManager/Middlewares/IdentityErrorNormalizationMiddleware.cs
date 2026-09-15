using System.Text.Json;

namespace Nexas.SystemManager.Middlewares
{
    /// <summary>
    /// Os endpoints de Identity (/api/v1/auth/*) vêm prontos do framework e retornam erros de validação
    /// no formato padrão do ASP.NET Core (ValidationProblemDetails), diferente do formato
    /// { statusCode, message, errors, stackTrace } usado pelo resto da API (ver GlobalExceptionMiddleware).
    /// Este middleware intercepta só essas rotas e traduz o erro para o mesmo formato e para mensagens em PT-BR.
    /// </summary>
    public class IdentityErrorNormalizationMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly Dictionary<string, (string Field, string Message)> KnownErrors = new()
        {
            ["DuplicateUserName"] = ("Email", "Este e-mail já está cadastrado."),
            ["DuplicateEmail"] = ("Email", "Este e-mail já está cadastrado."),
            ["InvalidUserName"] = ("Email", "O e-mail informado não é válido."),
            ["InvalidEmail"] = ("Email", "O e-mail informado não é válido."),
            ["PasswordTooShort"] = ("Password", "A senha informada é muito curta."),
            ["PasswordRequiresNonAlphanumeric"] = ("Password", "A senha precisa conter ao menos um caractere especial."),
            ["PasswordRequiresDigit"] = ("Password", "A senha precisa conter ao menos um número."),
            ["PasswordRequiresLower"] = ("Password", "A senha precisa conter ao menos uma letra minúscula."),
            ["PasswordRequiresUpper"] = ("Password", "A senha precisa conter ao menos uma letra maiúscula."),
            ["PasswordRequiresUniqueChars"] = ("Password", "A senha precisa conter mais caracteres únicos."),
            ["PasswordMismatch"] = ("Password", "A senha atual informada está incorreta."),
        };

        public IdentityErrorNormalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1/auth"))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status400BadRequest &&
                (context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                buffer.Seek(0, SeekOrigin.Begin);
                var rawBody = await new StreamReader(buffer).ReadToEndAsync();
                var normalized = TryNormalize(rawBody);

                context.Response.Body = originalBody;

                if (normalized != null)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength = null;
                    await context.Response.WriteAsync(normalized);
                    return;
                }

                await context.Response.WriteAsync(rawBody);
                return;
            }

            buffer.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBody;
            await buffer.CopyToAsync(originalBody);
        }

        private static string? TryNormalize(string rawBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                if (!doc.RootElement.TryGetProperty("errors", out var errorsElement) || errorsElement.ValueKind != JsonValueKind.Object)
                    return null;

                var errors = new List<object>();
                foreach (var prop in errorsElement.EnumerateObject())
                {
                    var rawMessage = prop.Value.EnumerateArray().Select(m => m.GetString() ?? string.Empty).FirstOrDefault() ?? prop.Name;

                    errors.Add(KnownErrors.TryGetValue(prop.Name, out var known)
                        ? new { PropertyName = known.Field, ErrorMessage = known.Message }
                        : new { PropertyName = prop.Name, ErrorMessage = rawMessage });
                }

                var response = new
                {
                    StatusCode = 400,
                    Message = "Um ou mais erros de validação ocorreram na sua requisição.",
                    Errors = errors,
                    StackTrace = (string?)null
                };

                return JsonSerializer.Serialize(response);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public static class IdentityErrorNormalizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdentityErrorNormalization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdentityErrorNormalizationMiddleware>();
        }
    }
}
