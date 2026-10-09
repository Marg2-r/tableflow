using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Models;

namespace TableFlow.Api.Data;

public static class IdentitySeeder
{
    private const string ManagerRole = "Manager";

    public static async Task SeedManagerAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var restaurantId =
            configuration.GetValue<int?>(
                "BootstrapAdmin:RestaurantId") ?? 1;

        var dbContext = services
            .GetRequiredService<TableFlowDbContext>();

        var restaurantExists =
            await dbContext.Restaurants.AnyAsync(
                restaurant =>
                    restaurant.Id == restaurantId);

        if (!restaurantExists)
        {
            throw new InvalidOperationException(
                $"Bootstrap restaurant {restaurantId} does not exist.");
        }

        var roleManager = services
            .GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(ManagerRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(ManagerRole));

            ThrowIfFailed(roleResult, "create Manager role");
        }

        var userManager = services
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                EmailConfirmed = true,
                RestaurantId = restaurantId
            };

            var userResult = await userManager.CreateAsync(
                user,
                password);

            ThrowIfFailed(userResult, "create bootstrap manager");
        }

        if (!await userManager.IsInRoleAsync(user, ManagerRole))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user,
                ManagerRole);

            ThrowIfFailed(roleResult, "assign Manager role");
        }
    }

    private static void ThrowIfFailed(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error => error.Description));

        throw new InvalidOperationException(
            $"Could not {operation}: {errors}");
    }
}