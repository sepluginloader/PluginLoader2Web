using Microsoft.EntityFrameworkCore;
using PluginLoader2Web.Data.Models;
using System.Threading.Tasks;

namespace PluginLoader2Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }

        public DbSet<PluginProjectItem> PluginProjects { get; set; }
        public DbSet<PluginVersionItem> UserAccountPolicys { get; set; }
        public DbSet<UserAccountItem> UserAccounts { get; set; }

        public async Task<UserAccountItem?> TryFindUser(ulong userId)
        {
            return await UserAccounts.FirstOrDefaultAsync(x => x.GithubID == userId);
        }


    }
}
