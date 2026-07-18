using System.Text.Json.Serialization;

namespace TableFlow.Api.Models;

public class RestaurantTable
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Zone { get; set; } = string.Empty;

    public int XPosition { get; set; }

    public int YPosition { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }
}