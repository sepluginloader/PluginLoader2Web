using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using PluginLoader2Web.Data;
using PluginLoader2Web.Data.Models;

namespace PluginLoader2Web.Components.Account
{
    internal static class IdentityComponentsEndpoints
    {
        private static string GetRoleName(byte role)
        {
            return role switch
            {
                0 => "User",
                1 => "Author",
                2 => "Moderator",
                5 => "Admin",
                _ => "Unknown"
            };
        }

        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            accountGroup.MapPost("/PerformExternalLogin",PerformExternalLogin);
            accountGroup.MapGet("/ExternalLogin", HandleExternalLogin);
            accountGroup.MapGet("/Logout", Logout);

            var PluginsGroup = endpoints.MapGroup("/Plugins");
#if DEBUG
            PluginsGroup.MapGet("/GenerateData", GenerateRandomPluginData);
#endif




            return accountGroup;
        }

        private static async Task<IResult> PerformExternalLogin(HttpContext context, [FromQuery] string? returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";


            IEnumerable<KeyValuePair<string, StringValues>> query = [
                new("ReturnUrl", returnUrl),
                    new("Action", "ExternalLoginCallback")];

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                "/Account/ExternalLogin",
                QueryString.Create(query));

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };


            return Results.Challenge(properties, new[] { "GitHub" });
        }

        /// <summary>
        /// Oauth Finished. Check to see if user is in our Database. If not then create user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private static async Task<IResult> HandleExternalLogin(HttpContext context, [FromQuery] string returnUrl)
        {
            var authenticateResult = await context.AuthenticateAsync("GitHub");

            if (!authenticateResult.Succeeded)
            {
                return Results.Redirect("/Login");
            }

            // Extract user information from the external provider
            var claims = authenticateResult.Principal?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email == null)
            {
                return Results.Redirect("/Login");
            }



            /* Perform DB Lookup */
            ApplicationDbContext myDatabase = context.RequestServices.GetService<ApplicationDbContext>();

            string NameIdentifier = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ulong userId = ulong.Parse(NameIdentifier);

            var FoundUser = await myDatabase.TryFindUser(userId);

            byte RoleNumber = 0;
            if (FoundUser == null)
            {
                string username = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
       

                UserAccountItem newUser = new UserAccountItem(userId, username, email, 0);
                myDatabase.Add(newUser);
                await myDatabase.SaveChangesAsync();

                Console.WriteLine($"User {userId} not found in database. Creating new user.");
            }
            else
            {
                RoleNumber = FoundUser.Role;
            }

            //Create roll claim
            var roleClaim = new Claim(ClaimTypes.Role, GetRoleName(RoleNumber));
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            claimsIdentity.AddClaim(roleClaim);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return Results.Redirect("/");
        }


        /// <summary>
        /// Logs out the user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private static async Task<IResult> Logout(HttpContext context, [FromQuery] string? returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect(returnUrl);
        }

        private static async Task<IResult> GenerateRandomPluginData(HttpContext context)
        {
            // Generate random plugin data and save it to the database
            // This is just a placeholder for your actual implementation

            ApplicationDbContext DbContext = context.RequestServices.GetService<ApplicationDbContext>();


            for(int i = 0; i <= 10; i++)
            {

                PluginProjectItem newPlugin = new PluginProjectItem();
                newPlugin.Name = $"Plugin {i}";
                newPlugin.AuthorID = 52760019;
                newPlugin.ToolTip = $"This is a tooltip for Plugin {i}";
                newPlugin.LongDescription = $"This is a long description for Plugin {i}.";
                newPlugin.RepoURL = "www.google.com";


                DbContext.PluginProjects.Add(newPlugin);



            }

            await DbContext.SaveChangesAsync();

            return Results.Ok("Random plugin data generated successfully.");
        }
    }
}
