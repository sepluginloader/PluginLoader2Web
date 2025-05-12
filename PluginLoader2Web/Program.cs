using Blazored.SessionStorage;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PluginLoader2Web.Components;
using PluginLoader2Web.Components.Account;
using PluginLoader2Web.Data;
using PluginLoader2Web.Data.Configs;
using System.Reflection;

namespace PluginLoader2Web
{
    public class Program
    {
        public static string AppFolder = AppContext.BaseDirectory;


        public static async Task Main(string[] args)
        {
            Log.Init(Path.Combine(AppFolder, "logs", "pluginloader.log"));

            ConfigFile? config = await ConfigFile.TryLoadAsync(Path.Combine(AppFolder, "ServerConfigs.toml"));
            if (config == null)
                return;

            DatabaseService database = new DatabaseService(config.Database);
            await database.InitDatabase();

            await RunWebServer(config, database);
        }

        public static async Task RunWebServer(ConfigFile config, DatabaseService db)
        {
            ResetEnvironment(); // We use ConfigFile for all settings
            WebApplicationBuilder builder = WebApplication.CreateBuilder([]);
            builder.Configuration.Sources.Clear();

            Log.Link(builder.Host, "Web");

            config.Web.ApplySettings(builder);

            builder.Host.UseSystemd();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorization();
            builder.Services.AddBlazoredSessionStorage();


            /* SQL Services and Migrator */
            builder.Services.AddSingleton(db);

            /* Web Authentication */
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.Cookie.Name = "authToken";
                   options.LoginPath = "/Login";
                   options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                   options.AccessDeniedPath = "/Account/AccessDenied";
               }).AddGitHub(githubOptions =>
               {
                   githubOptions.ClientId = config.Github.ClientID;
                   githubOptions.ClientSecret = config.Github.ClientSecret;
                   githubOptions.Scope.Add("user:email");
                   githubOptions.SaveTokens = true;
                   
               });


            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                if (config.Web.Hsts && config.Web.HttpsPort > 0)
                {
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                    app.UseHttpsRedirection();
                }
            }


            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

            app.MapAdditionalIdentityEndpoints();

            await app.RunAsync();
        }

        private static void ResetEnvironment()
        {
            string? appMode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            foreach (var env in Environment.GetEnvironmentVariables().Keys)
            {
                if (env is string env_key && env_key.StartsWith("ASPNET"))
                    Environment.SetEnvironmentVariable(env_key, null);
            }

            if (string.IsNullOrWhiteSpace(appMode))
            {
#if DEBUG
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#endif
            }
            else
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", appMode);
            }
        }
    }
}