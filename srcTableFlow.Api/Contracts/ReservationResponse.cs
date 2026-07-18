using TableFlow.Api.Enums;

namespace TableFlow.Api.Contracts;

public class ReservationResponse
{
    public int Id { get; set; }

    public int RestaurantId { get; set; }

    public int TableId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public int GuestCount { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    public DateTime TableAvailableAtUtc { get; set; }

    public int DurationMinutes { get; set; }

    public int TurnoverBufferMinutes { get; set; }

    public ReservationStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
}