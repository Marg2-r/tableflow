using System.Text.Json.Serialization;

namespace TableFlow.Api.Models;

public class TableBlock
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CancelledAtUtc { get; set; }

    [JsonIgnore]
    public RestaurantTable? Table { get; set; }
}