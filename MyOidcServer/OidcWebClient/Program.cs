using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace OidcWebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Load config theo môi trường
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                 .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                                 .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add OpenIdConnect authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = builder.Configuration["OpenIDConnectSettings:Authority"];
                options.ClientId = builder.Configuration["OpenIDConnectSettings:ClientId"];
                options.ClientSecret = builder.Configuration["OpenIDConnectSettings:ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                options.TokenValidationParameters.SignatureValidator = delegate (string token, TokenValidationParameters validationParameters)
                {
                    return new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token);
                };
            });

            // Chọn port từ env hoặc để dev tự pick
            var portEnv = Environment.GetEnvironmentVariable("CLIENT_PORT");
            if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(port); // Docker/CI sẽ set PORT cố định
                });
            }

            // Base URL server: nếu không set thì client dev local phải detect
            var serverUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost"; // dev tự pick server port
            builder.Services.AddHttpClient("oidc", client =>
            {
                client.BaseAddress = new Uri(serverUrl);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
