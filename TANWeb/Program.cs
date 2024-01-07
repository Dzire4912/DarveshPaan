using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TAN.ApplicationCore;
using TAN.DomainModels.Entities;
using TAN.DomainModels.Helpers.AppSettings;
using TAN.Repository.Abstractions;
using TAN.Repository.Implementations;
using TANWeb.Helpers;
using TANWeb.Permission;
using System.Globalization;
using TANWeb.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TAN.Repository.Serilogs;
using static TANWeb.Helpers.IMailHelper;
using TAN.DomainModels.Helpers;
using Serilog;
using TANWeb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using TANWeb.Areas.PBJSnap.PBJSnapServices;
using static TANWeb.Areas.PBJSnap.PBJSnapServices.IPdfExportHelper;
using TANWeb.Areas.Inventory.Services;
using AspNetCore.ReCaptcha;
using TANWeb.Models;
using System.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using static TANWeb.Helpers.IEmailHelper;

var builder = WebApplication.CreateBuilder(args);
// Set up configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

#region Initialize logger
LoggerManager.InitializeLogger(ConnectionString: builder.Configuration["ConnectionStrings:Defaultcon"]);
#endregion
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SftpServices>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<IEmailHelper, EmailHelper>();
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Defaultcon")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPdfExportHelper, PdfExportHelper>();
builder.Services.AddScoped<IMailContentHelper, MailContentHelper>();
builder.Services.AddDataProtection();
//builder.Services.AddDefaultIdentity<AspNetUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DatabaseContext>();

// added Iupload interface
builder.Services.AddScoped<IUpload, UploadService>();
builder.Services.AddTransient<IGoogleCaptcha, GoogleCaptchaService>();
builder.Services.Configure<ReCaptcha>(builder.Configuration.GetSection("ReCaptcha"));
builder.Services.AddHttpContextAccessor();


builder.Services.AddRazorPages();
builder.Services.AddIdentity<AspNetUser, AspNetRoles>().
    AddEntityFrameworkStores<DatabaseContext>().
    AddDefaultUI().
    AddDefaultTokenProviders();
// Register custom role manager
builder.Services.AddScoped<RoleManager<AspNetRoles>, CustomRoleManager<AspNetRoles>>();
builder.Services.AddScoped<IRoleValidator<AspNetRoles>, CustomRoleValidator<AspNetRoles>>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._$@+";
    options.User.RequireUniqueEmail = true;

});
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = "ThinkAnewCookie";
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/accessdenied";
    options.LogoutPath = "/Identity/Account/Logout";
    options.SlidingExpiration = true;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    // Remove permission claims from the cookie
    options.Events = new CookieAuthenticationEvents
    {
        OnSigningIn = async context =>
        {
            var claimsIdentity = (ClaimsIdentity)context.Principal.Identity;
            var permissionClaimList = claimsIdentity.FindAll("Permission").ToList();
            if (permissionClaimList.Count > 0)
            {
                foreach (var claim in permissionClaimList)
                {
                    claimsIdentity.RemoveClaim(claim);
                }
            }
            context.HttpContext.Session.SetString("is-valid-login", "true");
        },
        OnSigningOut = async context =>
        {
            context.HttpContext.Session.Clear();
        },
        OnValidatePrincipal = async context =>
        {
            // Check if there's no authentication cookie
            if (!context.Request.Cookies.ContainsKey("ThinkAnewCookie"))
            {

                // Reject the principal and sign out if the cookie is missing
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            var sessionExists = context.HttpContext.Session.GetString("is-valid-login");

            if (string.IsNullOrEmpty(sessionExists))
            {
                // If the cookie or session doesn't exist, deny the request
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized Access");
                return;
            }
        }
    };
});
builder.Services.Configure<PasswordHasherOptions>(option =>
{
    option.IterationCount = 12000;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(t =>
{
    t.IdleTimeout = TimeSpan.FromMinutes(30);
    t.Cookie.HttpOnly = true;
    t.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCypressOrigin",
        builder =>
        {
            builder.WithOrigins("https://pbjsnapweb01-qa.azurewebsites.net/")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "AddPolicy", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromMinutes(1);
    }).RejectionStatusCode = 429);

builder.Services.AddHttpClient();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromSeconds(1);
});
builder.Services.AddAntiforgery(opts => opts.Cookie.Name = "ThinkAnewAntiforgeryCookie");
builder.Services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "ThinkAnew");
builder.Services.AddSingleton<DecryptionService>();
builder.Services.AddSingleton<CsvReaderService>();
builder.Services.AddSingleton<KronosPunchExportService>();
builder.Services.AddHostedService<TimedHostedService>();
//builder.Services.AddSingleton<SharedBgService>();

builder.Services.AddHostedService<TelecomReportingBackgroundService>();
var app = builder.Build();
IConfiguration configuration = app.Configuration;
TANAppSettings.SetAppSettings(configuration);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<AspNetUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AspNetRoles>>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>(); // Obtain an instance of IUnitOfWork
        //Roles user seeding
        await TANWeb.Seeds.SeedRoles.SeedAsync(userManager, roleManager);
        //SuperAdmin,Admin, User seeding
        await TANWeb.Seeds.DefaultUsers.SeedSuperAdminAsync(userManager, roleManager, unitOfWork);
        await TANWeb.Seeds.DefaultUsers.SeedBasicUserAsync(userManager, unitOfWork);
        await TANWeb.Seeds.DefaultUsers.SeedAdminUserAsync(userManager, unitOfWork);
    }
    catch (Exception ex)
    {
        Log.Error("", ex.ToString());
        Log.Error("Seeding", ex.Message);
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseCors("AllowCypressOrigin");
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[]
{
 new CultureInfo(SupportedCulture.DefaultCulture)
};

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' fonts.googleapis.com; " +
    "script-src 'self' 'unsafe-inline' 'unsafe-eval' cdnjs.cloudflare.com cdn.jsdelivr.net https://www.google.com https://www.gstatic.com https://js.live.net; " +
    "style-src 'self' 'unsafe-inline' fonts.googleapis.com cdnjs.cloudflare.com; " +
    "img-src 'self' 'unsafe-inline' blob: data:; " +
    "font-src 'self' 'unsafe-inline' fonts.gstatic.com data:; " +
    "connect-src 'self' *; " + // Allow connections from any origin
    "frame-src 'self' https://www.google.com/ blob:; " +
    "frame-ancestors 'self';");

    await next();
});
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(SupportedCulture.DefaultCulture),
    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,
    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});
StaticFileOptions staticFileOptionsForScripts = new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "Scripts")),
    RequestPath = "/Scripts"
};
app.UseStaticFiles(staticFileOptionsForScripts);
app.MapControllerRoute(
    name: "Inventory",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "LabourFileMover",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "PBJSnap",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "TelecomReporting",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "TANUserManagement",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
