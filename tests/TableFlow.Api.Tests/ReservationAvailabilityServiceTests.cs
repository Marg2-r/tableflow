using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;
using TableFlow.Api.Models;
using TableFlow.Api.Services;

namespace TableFlow.Api.Tests;

public sealed class ReservationAvailabilityServiceTests
{
    [Theory]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Seated)]
    public async Task OverlappingBlockingReservation_PreventsDoubleBooking(
        ReservationStatus status)
    {
        await using var dbContext = CreateDbContext();

        dbContext.Reservations.Add(
            CreateReservation(status));

        await dbContext.SaveChangesAsync();

        var service = new ReservationAvailabilityService(dbContext);
        var candidateWindow = new ReservationWindow(
            Utc(19, 0),
            Utc(21, 0),
            Utc(21, 15));

        var isAvailable = await service.IsTableAvailableAsync(
            tableId: 10,
            candidateWindow);

        Assert.False(isAvailable);
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.NoShow)]
    [InlineData(ReservationStatus.Completed)]
    public async Task OverlappingTerminalReservation_DoesNotBlockTable(
        ReservationStatus status)
    {
        await using var dbContext = CreateDbContext();

        dbContext.Reservations.Add(
            CreateReservation(status));

        await dbContext.SaveChangesAsync();

        var service = new ReservationAvailabilityService(dbContext);
        var candidateWindow = new ReservationWindow(
            Utc(19, 0),
            Utc(21, 0),
            Utc(21, 15));

        var isAvailable = await service.IsTableAvailableAsync(
            tableId: 10,
            candidateWindow);

        Assert.True(isAvailable);
    }

    [Fact]
    public async Task ReservationStartingAfterTurnover_DoesNotConflict()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Reservations.Add(
            CreateReservation(ReservationStatus.Confirmed));

        await dbContext.SaveChangesAsync();

        var service = new ReservationAvailabilityService(dbContext);
        var candidateWindow = new ReservationWindow(
            Utc(20, 15),
            Utc(22, 15),
            Utc(22, 30));

        var isAvailable = await service.IsTableAvailableAsync(
            tableId: 10,
            candidateWindow);

        Assert.True(isAvailable);
    }

    private static TableFlowDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<TableFlowDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new TableFlowDbContext(options);
    }

    private static Reservation CreateReservation(
        ReservationStatus status)
    {
        return new Reservation
        {
            Id = 1,
            RestaurantId = 1,
            TableId = 10,
            CustomerName = "Test Guest",
            CustomerEmail = "guest@example.com",
            CustomerPhone = "+420000000000",
            GuestCount = 2,
            StartsAtUtc = Utc(18, 0),
            EndsAtUtc = Utc(20, 0),
            TableAvailableAtUtc = Utc(20, 15),
            Status = status,
            CreatedAtUtc = Utc(12, 0),
            UpdatedAtUtc = Utc(12, 0)
        };
    }

    private static DateTime Utc(int hour, int minute)
    {
        return new DateTime(
            2026,
            10,
            10,
            hour,
            minute,
            0,
            DateTimeKind.Utc);
    }
}
