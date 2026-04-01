using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

if(builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}    
// Add OpenID Connect authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie() // cookie used for local sign-in session
.AddOpenIdConnect(options =>
{
    // configure from configuration (put these values in appsettings.json or user secrets)
    options.Authority = builder.Configuration["OpenIDConnectSettings:Authority"];
    options.ClientId = builder.Configuration["OpenIDConnectSettings:ClientId"];
    options.ClientSecret = builder.Configuration["OpenIDConnectSettings:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    // scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // optional: map additional claims, events, etc.
    // options.ClaimActions.MapJsonKey("custom_claim", "custom_claim");
    // options.Events = new OpenIdConnectEvents { ... };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
