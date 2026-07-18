namespace TableFlow.Api.Services;

public sealed record ReservationWindow(
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    DateTime TableAvailableAtUtc);