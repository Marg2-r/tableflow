using System.Text.Json.Serialization;

namespace TableFlow.Api.Models;

public class OpeningHours
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpeningTime { get; set; }

    public TimeOnly ClosingTime { get; set; }

    public bool IsClosed { get; set; }

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }
}