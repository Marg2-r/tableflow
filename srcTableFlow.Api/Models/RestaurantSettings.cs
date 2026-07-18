using System.Text.Json.Serialization;

namespace TableFlow.Api.Models;

public class RestaurantSettings
{
    public int RestaurantId { get; set; }

    public int DefaultReservationDurationMinutes { get; set; } = 120;  // максимальное время резервации

    public int SlotIntervalMinutes { get; set; } = 15; // интервалы бронирования 16:00 16:15

    public int TurnoverBufferMinutes { get; set; } = 0; // время после закрытия стола на уборку

    public int MinimumAdvanceMinutes { get; set; } = 30; // минимакльная резервация по времени 

    public int MaximumAdvanceDays { get; set; } = 90; // на сколько дне в перед можно сделать резервацию 

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }
}