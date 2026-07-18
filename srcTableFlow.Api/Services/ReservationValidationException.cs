namespace TableFlow.Api.Services;

public sealed class ReservationValidationException : Exception
{
    public ReservationValidationException(string message)
        : base(message)
    {
    }
}