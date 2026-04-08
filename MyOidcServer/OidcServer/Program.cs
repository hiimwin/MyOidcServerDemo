using Microsoft.IdentityModel.Tokens;
using OidcServer.Helpers;
using OidcServer.Models;
using OidcServer.Repositories;

namespace OidcServer
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

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            builder.Services.AddSingleton<ICodeItemRepository, CodeItemRepository>();

            var tokenIssuingOptions = builder.Configuration.GetSection("TokenIssuing").Get<TokenIssuingOptions>() ?? new TokenIssuingOptions();
            builder.Services.AddSingleton(tokenIssuingOptions);
            builder.Services.AddSingleton(JwkLoader.LoadFromDefault());

            // Chỉ định Kestrel nếu muốn, nhưng local dev cứ để .NET Core tự pick
            var portEnv = Environment.GetEnvironmentVariable("SERVER_PORT");
            if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var port))
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(port); // Docker/CI sẽ set PORT cố định
                });
            }

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
            app.UseAuthorization();

            app.MapGet("/.well-known/openid-configuration",
                () => Results.File(Path.Combine(builder.Environment.ContentRootPath, "OidcDiscovery", "openid-configuration.json"), contentType: "application/json")
            );

            app.MapGet("/.well-known/jwks.json",
                () => Results.File(Path.Combine(builder.Environment.ContentRootPath, "OidcDiscovery", "jwks.json"), contentType: "application/json")
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
