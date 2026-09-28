using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TableFlow.Api.Data;
using TableFlow.Api.Services;
using Microsoft.AspNetCore.Identity;
using TableFlow.Api.Models;
using Microsoft.AspNetCore.Authorization;
using TableFlow.Api.Security;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var allowedOrigins = builder.Configuration
    .GetSection("Frontend:AllowedOrigins")
    .Get<string[]>()
    ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Frontend:AllowedOrigins is not configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

builder.Services.AddDbContext<TableFlowDbContext>(options =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString);
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<TableFlowDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "tableflow.manager";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    // API не должен перенаправлять на HTML-страницу входа.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        TableFlowPolicies.ManagerRestaurant,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Manager");
            policy.AddRequirements(
                new RestaurantAccessRequirement());
        });
});

builder.Services.AddSingleton<
    IAuthorizationHandler,
    RestaurantAccessHandler>();

builder.Services
    .AddScoped<ReservationAvailabilityService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<TableFlowDbContext>();

    await dbContext.Database.MigrateAsync();

    await IdentitySeeder.SeedManagerAsync(
    scope.ServiceProvider,
    app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tableflow API v1");
    });

}
//Leave this disabled for now because you are running HTTP locally
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

