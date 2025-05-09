using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace PluginLoader2Web.Components.Account
{
    internal static class IdentityComponentsEndpoints
    {
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            accountGroup.MapPost("/PerformExternalLogin", (
                HttpContext context,
                [FromForm] string provider,
                [FromForm] string returnUrl) =>
            {
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


                return Results.Challenge(properties, new[] { provider });
            });


            accountGroup.MapGet("/ExternalLogin", async (
              HttpContext context,
              [FromQuery] string returnUrl) =>
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

                // Here you would typically look up the user in your database and create a user if they don't exist
                // For simplicity, we'll assume the user exists and sign them in

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return Results.Redirect("/");
            });

            return accountGroup;
        }
    }
}
