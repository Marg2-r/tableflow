using System.ComponentModel.DataAnnotations;

namespace TableFlow.Api.Contracts;

public class UpdateRestaurantSettingsRequest
{
    [Range(15, 720)]
    public int DefaultReservationDurationMinutes { get; set; }

    [Range(5, 60)]
    public int SlotIntervalMinutes { get; set; }

    [Range(0, 180)]
    public int TurnoverBufferMinutes { get; set; }

    [Range(0, 10080)]
    public int MinimumAdvanceMinutes { get; set; }

    [Range(1, 365)]
    public int MaximumAdvanceDays { get; set; }
}