using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.helpers;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.repository;
using runningClob.repository.ClubRepository;
using runningClob.repository.RaceRepositroy;
using runningClob.repository.UserRepository;
using runningClob.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<ICountryAliasService, CountryAliasService>();

builder.Services.AddScoped<IRaceRepository, RaceRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHttpClient<IGeolocationService, IPInfoService>();
builder.Services.AddScoped<IPhotoService, PhotoServices>();
builder.Services.AddHttpClient<IPInfoService>(client =>
{
    client.BaseAddress = new Uri("https://ipinfo.io/");
    client.DefaultRequestHeaders.Add("User-Agent", "RunningClubApp/1.0");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["IPInfo:Token"]}");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with AppUser
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>(); // Enable Role Management
builder.Services.AddSession();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
var app = builder.Build();

// Seed Data
if (args.Length == 1 && string.Equals(args[0], "seeddata", StringComparison.OrdinalIgnoreCase))
{
    await Seed.SeedUsersAndRolesAsync(app);
    Seed.SeedData(app); // Updated to async
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();