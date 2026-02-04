using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.User;
using our_group.Infrastructure.Data;
using our_group.Infrastructure;
using our_group.Infrastructure.Hubs;
using our_group.LocationDomain.Core.DTOs;
using our_group.LocationDomain.Core.Interfaces;
using our_group.LocationDomain.Application.Services;
using our_group.LocationDomain.Infrastructure.Data;
using our_group.Core.Domain.User.Services;
using our_group.Core.Domain.User;
var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();

builder.Services.AddDbContext<LocationContext>(o =>
    o.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "location")));
builder.Services.AddDbContext<GameContext>(o =>
    o.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "game")));
builder.Services.AddDbContext<UserContext>(o =>
    o.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "user")));
builder.Services.AddDbContext<LocationDbContext>(options =>
    options.UseNpgsql(conn, npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "location"))
);

builder.Services.Configure<GoogleMapsSettings>(
   builder.Configuration.GetSection("GoogleMaps")
   );

builder.Services.AddHttpClient<IGooglePlacesService, GooglePlacesService>((serviceProvider, client) =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSignalR();

// Register the Google maps services here
builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection("GoogleMaps"));

// MediatR from both assemblies
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(UserAccount).Assembly);
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<GameEngine>();

// Authentication + Authorization
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "CombinedCookies";
        options.DefaultAuthenticateScheme = "CombinedCookies";
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("CombinedCookies", "Combined Cookies", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            // If the admin auth cookie is present, authenticate using AdminCookie; otherwise use the user cookie
            return context.Request.Cookies.ContainsKey("AdminAuth")
                ? "AdminCookie"
                : CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";
        options.LogoutPath = "/Users/Logout";
        options.AccessDeniedPath = "/Users/Login";
    })
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.Cookie.Name = "AdminAuth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Repositories and services
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
// JWT not used; remove token service
builder.Services.AddScoped<IUserProfileReader, EfUserProfileReader>();
builder.Services.AddScoped<our_group.Core.Domain.Admin.IAdminRepository, our_group.Infrastructure.Admin.EfAdminRepository>();
builder.Services.AddScoped<our_group.Core.Domain.User.IPlayerProfileReader, our_group.Infrastructure.EfUserProfileReader>();
//builder.Services.AddScoped<our_group.Core.Domain.Game.FakedData.IUserInfoService, our_group.Core.Domain.User.Services.UserInfoService>();
builder.Services.AddScoped<our_group.Core.Domain.User.Services.IUserInfoService, our_group.Core.Domain.User.Services.UserInfoService>();
builder.Services.AddScoped<IGooglePlacesService, GooglePlacesService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();



builder.Services.AddLogging();

var app = builder.Build();

// Apply UserContext migrations and seed default admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserContext>();
    await db.Database.MigrateAsync();
    var adminRepo = scope.ServiceProvider.GetRequiredService<our_group.Core.Domain.Admin.IAdminRepository>();
    if (!await adminRepo.ExistsAsync())
    {
        var initialUser = builder.Configuration["Admin:Username"] ?? "admin";
        var initialPass = builder.Configuration["Admin:InitialPassword"] ?? "admin";
        await adminRepo.CreateAsync(initialUser, initialPass, mustChangePassword: true);
    }

    // Seed a default regular user if configured
    var seedUser = builder.Configuration["SeedUser:Username"];
    var seedPass = builder.Configuration["SeedUser:Password"];
    var seedEmail = builder.Configuration["SeedUser:Email"];
    if (!string.IsNullOrWhiteSpace(seedUser) && !string.IsNullOrWhiteSpace(seedPass) && !string.IsNullOrWhiteSpace(seedEmail))
    {
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (!await users.ExistsByUsernameAsync(seedUser) && !await users.ExistsByEmailAsync(seedEmail))
        {
            await users.CreateAsync(seedUser, seedEmail, seedPass);
        }
    }

    // Optional: seed a second default user
    var seedUser2 = builder.Configuration["SeedUser2:Username"];
    var seedPass2 = builder.Configuration["SeedUser2:Password"];
    var seedEmail2 = builder.Configuration["SeedUser2:Email"];
    if (!string.IsNullOrWhiteSpace(seedUser2) && !string.IsNullOrWhiteSpace(seedPass2) && !string.IsNullOrWhiteSpace(seedEmail2))
    {
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (!await users.ExistsByUsernameAsync(seedUser2) && !await users.ExistsByEmailAsync(seedEmail2))
        {
            await users.CreateAsync(seedUser2, seedEmail2, seedPass2);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

// Serve files from wwwroot (css/js/images)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<GameHub>("/gamehub"); // Makes /gamehub the websocket URL!

// Removed JWT minimal API endpoints

app.Run();
