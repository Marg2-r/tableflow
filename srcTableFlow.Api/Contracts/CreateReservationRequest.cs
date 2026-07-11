using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TableFlow.Api.Contracts;

public class CreateReservationRequest
{
    [Range(1, int.MaxValue, ErrorMessage ="TableId must be greater than 0.")]
    public int TableId { get; set; }

    [Required(ErrorMessage ="Customer name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 100 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer email is required.")]
    [EmailAddress(ErrorMessage = "Customer email is not valid.")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer phone is required.")]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "Customer phone must be between 5 and 30 characters.")]
    public string CustomerPhone { get; set; } = string.Empty;

    public DateOnly ReservationDate { get; set; }

    public TimeOnly ReservationTime { get; set; }

    [Range(1, 20, ErrorMessage = "Guest count must be between 1 and 20.")]
    public int GuestCount { get; set; }
}
