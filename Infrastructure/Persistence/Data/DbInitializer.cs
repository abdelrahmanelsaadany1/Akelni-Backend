using Domain.Contracts;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IdentityContext _identityContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(IdentityContext identityContext, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _identityContext = identityContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializerIdentityAsync()
        {
            // ✅ 1) Apply pending migrations
            await _identityContext.Database.MigrateAsync();

            // ✅ 2) Create Roles if they don't exist
            string[] roles = ["Admin", "Chef", "Customer"];
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}