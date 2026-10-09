using TableFlow.Api.Enums;
using TableFlow.Api.Services;

namespace TableFlow.Api.Tests;

public sealed class ReservationStatusTransitionsTests
{
    [Theory]
    [InlineData(
        ReservationStatus.Confirmed,
        ReservationStatus.Seated)]
    [InlineData(
        ReservationStatus.Confirmed,
        ReservationStatus.NoShow)]
    [InlineData(
        ReservationStatus.Confirmed,
        ReservationStatus.Cancelled)]
    [InlineData(
        ReservationStatus.Seated,
        ReservationStatus.Completed)]
    public void CanTransition_ReturnsTrue_ForAllowedTransition(
        ReservationStatus currentStatus,
        ReservationStatus nextStatus)
    {
        var result = ReservationStatusTransitions.CanTransition(
            currentStatus,
            nextStatus);

        Assert.True(result);
    }

    [Theory]
    [InlineData(
        ReservationStatus.Confirmed,
        ReservationStatus.Completed)]
    [InlineData(
        ReservationStatus.Seated,
        ReservationStatus.Cancelled)]
    [InlineData(
        ReservationStatus.Seated,
        ReservationStatus.NoShow)]
    [InlineData(
        ReservationStatus.Cancelled,
        ReservationStatus.Confirmed)]
    [InlineData(
        ReservationStatus.NoShow,
        ReservationStatus.Seated)]
    [InlineData(
        ReservationStatus.Completed,
        ReservationStatus.Seated)]
    public void CanTransition_ReturnsFalse_ForInvalidTransition(
        ReservationStatus currentStatus,
        ReservationStatus nextStatus)
    {
        var result = ReservationStatusTransitions.CanTransition(
            currentStatus,
            nextStatus);

        Assert.False(result);
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.NoShow)]
    [InlineData(ReservationStatus.Completed)]
    public void CanTransition_ReturnsFalse_FromTerminalStatus(
        ReservationStatus terminalStatus)
    {
        foreach (var nextStatus in
                 Enum.GetValues<ReservationStatus>())
        {
            Assert.False(
                ReservationStatusTransitions.CanTransition(
                    terminalStatus,
                    nextStatus));
        }
    }
}
