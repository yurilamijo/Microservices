using Identity.Entities;
using Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Services
{
    public class IdentitySeedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IdentitySettings _identitySettings;

        public IdentitySeedHostedService(IServiceScopeFactory scopeFactory, IOptions<IdentitySettings> identityOptions)
        {
            _scopeFactory = scopeFactory;
            _identitySettings = identityOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await CreateRoleIfNotExistsAsync(Roles.Admin, roleManager);
            await CreateRoleIfNotExistsAsync(Roles.Member, roleManager);

            var adminUser = await userManager.FindByEmailAsync(_identitySettings.AdminUserEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = _identitySettings.AdminUserEmail,
                    Email = _identitySettings.AdminUserEmail,
                };

                await userManager.CreateAsync(adminUser, _identitySettings.AdminUserPassword);
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task CreateRoleIfNotExistsAsync(string role, RoleManager<ApplicationRole> roleManager)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}
