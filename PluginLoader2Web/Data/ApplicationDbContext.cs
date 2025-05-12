using Microsoft.EntityFrameworkCore;
using PluginLoader2Web.Data.Models;
using System.Threading.Tasks;

namespace PluginLoader2Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string connectionString;

        public ApplicationDbContext(string connectionString)
        {
            this.connectionString = connectionString;
            this.SaveChangesFailed += SQLServer_SaveChangesFailed;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

        private void SQLServer_SaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
        {
            Log.Error($"Failed to save changes to the database! {e.Exception}");
        }

        public DbSet<PluginProjectItem> PluginProjects { get; set; }
        public DbSet<PluginVersionItem> UserAccountPolicies { get; set; }
        public DbSet<UserAccountItem> UserAccounts { get; set; }

        public async Task<UserAccountItem?> TryFindUser(ulong userId)
        {
            return await UserAccounts.FirstOrDefaultAsync(x => x.GithubID == userId);
        }


    }
}
