using Microsoft.AspNetCore.Identity;

namespace TableFlow.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public int RestaurantId { get; set; }
}