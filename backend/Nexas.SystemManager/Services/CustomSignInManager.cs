using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Nexas.SystemManager.Services
{
    public class CustomSignInManager : SignInManager<NexasUser>
    {
        private readonly SystemManagerDbContext _dbContext;

        public CustomSignInManager(
            UserManager<NexasUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<NexasUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<NexasUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<NexasUser> confirmation,
            SystemManagerDbContext dbContext)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _dbContext = dbContext;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // Execute the standard password sign in process
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user != null)
                {
                    // Registra o login na nova tabela de histórico
                    var history = new UserLoginHistory
                    {
                        UserId = user.Id,
                        Method = "Forms"
                    };

                    _dbContext.LoginHistories.Add(history);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return result;
        }
    }
}
