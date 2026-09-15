using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Nexas.SystemManager.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Dev", "Admin", "Teacher", "Student", "Owner", "Customer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);

                    // Atribuindo as Claims iniciais para Admin e Owner
                    if (roleName == "Admin" || roleName == "Owner")
                    {
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", "ModuloFinanceiro"));
                        await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", "ModuloSuporte"));
                    }
                }
            }
        }
    }
}
