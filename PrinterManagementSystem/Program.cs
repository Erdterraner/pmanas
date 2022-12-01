using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Starterkit._keenthemes;
using Starterkit._keenthemes.libs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Starterkit.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("StarterkitContextConnection") ?? throw new InvalidOperationException("Connection string 'StarterkitContextConnection' not found.");

builder.Services.AddDbContext<StarterkitContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<StarterkitUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<StarterkitContext>();

builder.Services.AddDistributedMemoryCache();  
builder.Services.AddSession(options => {  
    options.IdleTimeout = TimeSpan.FromMinutes(1);
});
builder.Services.AddScoped<IKTTheme, KTTheme>();
builder.Services.AddSingleton<IKTBootstrapBase, KTBootstrapBase>();

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] {
            new CultureInfo("ja"),
            new CultureInfo("en"),
            new CultureInfo("ar"),
            new CultureInfo("de"),
            new CultureInfo("es"),
            new CultureInfo("fr"),
        };
        options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("themesettings.json")
                            .Build();

var app = builder.Build();

app.Use(async (context, next) =>
    {
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/not-found";
        await next();
    }
});

KTThemeSettings.init(configuration);

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
app.UseAuthentication();;

app.UseAuthorization();
app.UseSession();
app.UseThemeMiddleware();
app.MapControllerRoute(name: "signin",
                pattern: "signin",
                defaults: new { controller = "Auth", action = "signIn" });
app.MapControllerRoute(name: "signup",
                pattern: "signup",
                defaults: new { controller = "Auth", action = "signUp" });
app.MapControllerRoute(name: "reset-password",
                pattern: "reset-password",
                defaults: new { controller = "Auth", action = "resetPassword" });
app.MapControllerRoute(name: "new-password",
                pattern: "new-password",
                defaults: new { controller = "Auth", action = "newPassword" });

app.MapControllerRoute(name: "not-found",
                pattern: "not-found",
                defaults: new { controller = "System", action = "notFound" });
app.MapControllerRoute(name: "error",
                pattern: "error",
                defaults: new { controller = "System", action = "error" });

app.MapControllerRoute(name: "management",
    pattern: "management",
    defaults: new { controller = "Dashboards", action = "management" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboards}/{action=Index}/{id?}");

app.Run();
