using TableFlow.Api.Enums;

namespace TableFlow.Api.Contracts;

public sealed class UpdateReservationStatusRequest
{
    public ReservationStatus Status { get; set; }
}
