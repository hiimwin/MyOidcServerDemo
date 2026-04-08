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

            // Expose port 8080 trong container
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080);
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
