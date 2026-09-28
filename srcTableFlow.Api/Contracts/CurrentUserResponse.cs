namespace TableFlow.Api.Contracts;

public sealed class CurrentUserResponse
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int RestaurantId { get; set; }

    public List<string> Roles { get; set; } = [];
}