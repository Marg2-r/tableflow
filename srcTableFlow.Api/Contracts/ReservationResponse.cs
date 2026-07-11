using System;
using System.Collections.Generic;
using System.Text;
using TableFlow.Api.Enums;

namespace TableFlow.Api.Contracts;
public class ReservationResponse
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public DateOnly ReservationDate { get; set; }

    public TimeOnly ReservationTime { get; set; }

    public int GuestCount { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
