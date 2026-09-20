using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Nexas.SystemManager.Domain.Entities;

namespace Nexas.SystemManager.Services
{
    /// <summary>
    /// A claim de Name gerada pelo Identity por padrão usa o UserName (que aqui é sempre o
    /// e-mail, tanto no registro quanto no /google-login). Sobrescrevemos pelo FullName real
    /// do usuário, que é o que o frontend exibe (hero da home, navbar, etc.).
    /// </summary>
    public class NexasUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<NexasUser, IdentityRole>
    {
        public NexasUserClaimsPrincipalFactory(
            UserManager<NexasUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(NexasUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                var nameClaim = identity.FindFirst(Options.ClaimsIdentity.UserNameClaimType);
                if (nameClaim != null)
                {
                    identity.RemoveClaim(nameClaim);
                }

                identity.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, user.FullName));
            }

            return identity;
        }
    }
}
