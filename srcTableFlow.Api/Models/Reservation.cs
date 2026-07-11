using TableFlow.Api.Enums;

namespace TableFlow.Api.Models;

public  class Reservation
{
    public int Id { get; set; }
    public int TableId { get; set; }

    public string CustomerName {  get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public DateOnly ReservationDate { get; set; }

    public TimeOnly ReservationTime { get; set; }

    public int GuestCount { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


}
