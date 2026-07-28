using System;
using System.Collections.Generic;
using System.Text;

namespace TableFlow.Api.Contracts;


public class RestaurantSettingsResponse
{
    public int RestaurantId { get; set; }

    public int DefaultReservationDurationMinutes { get; set; }

    public int SlotIntervalMinutes { get; set; }

    public int TurnoverBufferMinutes { get; set; }

    public int MinimumAdvanceMinutes { get; set; }

    public int MaximumAdvanceDays { get; set; }
}