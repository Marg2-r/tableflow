using TableFlow.Api.Enums;

namespace TableFlow.Api.Services;

public static class ReservationStatusTransitions
{
    public static bool CanTransition(
        ReservationStatus currentStatus,
        ReservationStatus nextStatus)
    {
        return (currentStatus, nextStatus) switch
        {
            (ReservationStatus.Confirmed,
                ReservationStatus.Seated) => true,
            (ReservationStatus.Confirmed,
                ReservationStatus.NoShow) => true,
            (ReservationStatus.Confirmed,
                ReservationStatus.Cancelled) => true,
            (ReservationStatus.Seated,
                ReservationStatus.Completed) => true,
            _ => false
        };
    }
}
