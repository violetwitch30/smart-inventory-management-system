using SmartInventoryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;
using SmartInventoryManagementSystem.Areas.ProjectManagement.Models;
using SmartInventoryManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure connection string "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Serilog
// Logging Levels: Verbose, Debug, Information, Warning, Error, Fatal
Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug() // Only logs Debug and below
    //.WriteTo.Console()
    //.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true) // daily log files
    //.Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration) // read log configuration from appsettings
    .CreateLogger();

// Inject out Brevo email sender
builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseStatusCodePagesWithRedirects("/Home/NotFound?statusCode={0}");
}

using var scope = app.Services.CreateScope();
var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
try
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Seed initial roles
    await ContextSeed.SeedRolesAsync(userManager, roleManager);
    
    // Seed SuperUser
    await ContextSeed.SuperSeedRolesAsync(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred while seeding the database.");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
    
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name:"areas",
    pattern:"{area:exists}/{controller=Product}/{action=Index}/{id}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();