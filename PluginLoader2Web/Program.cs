using Blazored.SessionStorage;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public static ConfigFile MainConfigs;


        public static async Task Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
           

            MainConfigs = await ConfigFile.TryLoadAsync(Path.Combine(AppFolder, "ServerConfigs.toml"));
            

            //Build Web Services
            await BuildServicesAsync(builder, args);


        }

        public static async Task BuildServicesAsync(WebApplicationBuilder builder, string[] args)
        {
    

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorization();
            builder.Services.AddBlazoredSessionStorage();


            /* SQL Services and Migrator */
            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(MainConfigs.WebServer.ConnectionString)
                    .ScanIn(typeof(App).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(MainConfigs.WebServer.ConnectionString));


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
                   githubOptions.ClientId = MainConfigs.GithubOAuth.ClientID;
                   githubOptions.ClientSecret = MainConfigs.GithubOAuth.ClientSecret;
                   githubOptions.Scope.Add("user:email");
                   githubOptions.SaveTokens = true;
                   
               });





            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();


            //Run the migration service
            using (var scope = app.Services.CreateScope())
            {


                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.LoadVersionInfoIfRequired();
                runner.MigrateUp();
                runner.ListMigrations();
                runner.ValidateVersionOrder();

                await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().SaveChangesAsync();
            }

            app.MapAdditionalIdentityEndpoints();
            app.Run();
        }
    }
}
