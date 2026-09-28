using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TableFlow.Api.Security;

public static class TableFlowClaimTypes
{
    public const string RestaurantId = "restaurant_id";
}

public static class TableFlowPolicies
{
    public const string ManagerRestaurant = "ManagerRestaurant";
}

public sealed class RestaurantAccessRequirement
    : IAuthorizationRequirement
{
}

public sealed class RestaurantAccessHandler
    : AuthorizationHandler<RestaurantAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RestaurantAccessRequirement requirement)
    {
        var httpContext = context.Resource switch
        {
            HttpContext value => value,
            AuthorizationFilterContext value => value.HttpContext,
            _ => null
        };

        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var routeValue = httpContext.Request
            .RouteValues["restaurantId"]?
            .ToString();

        var claimValue = context.User
            .FindFirst(TableFlowClaimTypes.RestaurantId)?
            .Value;

        if (int.TryParse(routeValue, out var routeRestaurantId) &&
            int.TryParse(claimValue, out var userRestaurantId) &&
            routeRestaurantId == userRestaurantId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}