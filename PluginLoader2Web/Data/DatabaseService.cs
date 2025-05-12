using FluentMigrator.Runner;
using Npgsql;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using PluginLoader2Web.Data.Configs;

namespace PluginLoader2Web.Data
{
    public class DatabaseService
    {
        private string connectionString;

        public bool IsConnected { get; private set; }

        public DatabaseService(SQLCredentialsCfgs config)
        {
            connectionString = config.ConnectionString;
        }

        public async Task InitDatabase()
        {
            // Useful resource for writing migrations: https://www.npgsql.org/doc/types/basic.html

            try
            {
                using (ServiceProvider sp = GetMigratorService())
                {
                    IMigrationRunner runner = sp.GetRequiredService<IMigrationRunner>();
                    runner.LoadVersionInfoIfRequired();
                    runner.MigrateUp();
                    runner.ListMigrations();
                    runner.ValidateVersionOrder();
                }

                ApplicationDbContext sqlTest = new ApplicationDbContext(connectionString);
                await sqlTest.SaveChangesAsync();
                IsConnected = true;
                return;
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "28P01")
                    Log.Error("SQL Authentication Error! Please verify username and password is correct!");
                else
                    Log.Error("Error connecting to database:", ex);
            }
            catch (NpgsqlException ex)
            {
                if (ex.InnerException is SocketException)
                    Log.Error("Failed to connect to the database. The target machine actively refused it. Verify credentials and ports are correct by using the test button in the config setup");
                else
                    Log.Error("Error connecting to database:", ex);
            }
            IsConnected = false;
        }

        private ServiceProvider GetMigratorService()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
                .AddLogging(x => Log.Link(x, "DbMigrator"))
                .BuildServiceProvider(false);
        }

        public ApplicationDbContext OpenConnection()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Unable to connect to the database, database init failed.");
            return new ApplicationDbContext(connectionString);
        }
    }
}
