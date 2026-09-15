using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Nexas.SystemManager.Domain.Entities;

namespace Nexas.SystemManager.Endpoints
{
    /// <summary>
    /// O /register embutido do MapIdentityApi só aceita e-mail e senha (contrato fixo do
    /// framework), então o registro do Nexas é implementado aqui, com nome e telefone,
    /// mantendo a mesma rota e reaproveitando a atribuição automática de role
    /// (primeiro usuário = Admin+Dev, os demais = Owner) e o mesmo formato de erro
    /// já normalizado pelo IdentityErrorNormalizationMiddleware.
    /// </summary>
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapCustomRegister(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/register", async (RegisterUserRequest request, UserManager<NexasUser> userManager) =>
            {
                var validationErrors = new Dictionary<string, string[]>();
                if (string.IsNullOrWhiteSpace(request.Nome)) validationErrors["Nome"] = new[] { "O nome é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Email)) validationErrors["Email"] = new[] { "O e-mail é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Telefone)) validationErrors["Telefone"] = new[] { "O telefone é obrigatório." };
                if (string.IsNullOrWhiteSpace(request.Senha)) validationErrors["Senha"] = new[] { "A senha é obrigatória." };

                if (validationErrors.Count > 0)
                {
                    return Results.ValidationProblem(validationErrors);
                }

                var user = new NexasUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.Nome,
                    PhoneNumber = request.Telefone
                };

                var result = await userManager.CreateAsync(user, request.Senha);
                if (!result.Succeeded)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    return Results.ValidationProblem(errors);
                }

                var isFirstUser = userManager.Users.Count() == 1;
                var rolesToAssign = isFirstUser ? new[] { "Admin", "Dev" } : new[] { "Owner" };
                await userManager.AddToRolesAsync(user, rolesToAssign);

                return Results.Ok();
            })
            .AllowAnonymous()
            .Add(endpointBuilder =>
            {
                // MapIdentityApi também mapeia POST /register (só email+senha). Sem isso, as duas
                // rotas empatam e o ASP.NET Core derruba a requisição com AmbiguousMatchException.
                // Dar prioridade (Order menor) faz essa rota, mais específica, vencer sem ambiguidade.
                if (endpointBuilder is RouteEndpointBuilder routeEndpointBuilder)
                {
                    routeEndpointBuilder.Order = -1;
                }
            });

            return endpoints;
        }
    }
}
