using Microsoft.EntityFrameworkCore;
using PluginLoader2Web.Data.Models;

namespace PluginLoader2Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }

        public DbSet<PluginProjectItem> UserAccounts { get; set; }
        public DbSet<PluginVersionItem> UserAccountPolicys { get; set; }

    }
}
