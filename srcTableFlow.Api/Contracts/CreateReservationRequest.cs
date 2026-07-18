using System.ComponentModel.DataAnnotations;

namespace TableFlow.Api.Contracts;

public class CreateReservationRequest
{
    [Range(1, int.MaxValue)]
    public int TableId { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    public DateOnly ReservationDate { get; set; }

    public TimeOnly ReservationTime { get; set; }

    [Range(1, 50)]
    public int GuestCount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
