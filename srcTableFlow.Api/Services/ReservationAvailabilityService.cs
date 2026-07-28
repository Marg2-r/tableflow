using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;
using TableFlow.Api.Models;

namespace TableFlow.Api.Services;

public sealed class ReservationAvailabilityService
{
    private static readonly ReservationStatus[] BlockingStatuses =
    [
        ReservationStatus.Confirmed,
        ReservationStatus.Seated
    ];

    private readonly TableFlowDbContext _dbContext;

    public ReservationAvailabilityService(
        TableFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservationWindow> CreateWindowAsync(
        int restaurantId,
        DateOnly date,
        TimeOnly time,
        CancellationToken cancellationToken = default)
    {
        var context = await GetDayContextAsync(
            restaurantId,
            date,
            cancellationToken);

        var localStart = date.ToDateTime(time);

        // Если ресторан работает, например, с 18:00 до 02:00,
        // время 01:00 относится к следующему календарному дню.
        if (context.ClosingLocal.Date > context.OpeningLocal.Date &&
            localStart < context.OpeningLocal)
        {
            localStart = localStart.AddDays(1);
        }

        var validationError = ValidateLocalStart(
            context,
            localStart,
            DateTime.UtcNow);

        if (validationError is not null)
        {
            throw new ReservationValidationException(
                validationError);
        }

        return BuildWindow(context, localStart);
    }

    public async Task<bool> IsTableAvailableAsync(
        int tableId,
        ReservationWindow window,
        CancellationToken cancellationToken = default)
    {
        var hasReservationConflict =
            await _dbContext.Reservations.AnyAsync(
                reservation =>
                    reservation.TableId == tableId &&
                    BlockingStatuses.Contains(
                        reservation.Status) &&
                    reservation.StartsAtUtc <
                        window.TableAvailableAtUtc &&
                    reservation.TableAvailableAtUtc >
                        window.StartsAtUtc,
                cancellationToken);

        if (hasReservationConflict)
        {
            return false;
        }

        var hasBlockConflict =
            await _dbContext.TableBlocks.AnyAsync(
                tableBlock =>
                    tableBlock.TableId == tableId &&
                    tableBlock.CancelledAtUtc == null &&
                    tableBlock.StartsAtUtc <
                        window.TableAvailableAtUtc &&
                    tableBlock.EndsAtUtc >
                        window.StartsAtUtc,
                cancellationToken);

        return !hasBlockConflict;
    }

    public async Task<List<RestaurantTable>>
        GetAvailableTablesAsync(
            int restaurantId,
            DateOnly date,
            TimeOnly time,
            int guests,
            CancellationToken cancellationToken = default)
    {
        if (guests < 1)
        {
            throw new ReservationValidationException(
                "Guest count must be at least 1.");
        }

        var window = await CreateWindowAsync(
            restaurantId,
            date,
            time,
            cancellationToken);

        return await _dbContext.Tables
            .AsNoTracking()
            .Where(table =>
                table.RestaurantId == restaurantId &&
                table.IsActive &&
                table.Capacity >= guests)
            .Where(table =>
                !_dbContext.Reservations.Any(reservation =>
                    reservation.TableId == table.Id &&
                    BlockingStatuses.Contains(
                        reservation.Status) &&
                    reservation.StartsAtUtc <
                        window.TableAvailableAtUtc &&
                    reservation.TableAvailableAtUtc >
                        window.StartsAtUtc))
            .Where(table =>
                !_dbContext.TableBlocks.Any(tableBlock =>
                    tableBlock.TableId == table.Id &&
                    tableBlock.CancelledAtUtc == null &&
                    tableBlock.StartsAtUtc <
                        window.TableAvailableAtUtc &&
                    tableBlock.EndsAtUtc >
                        window.StartsAtUtc))
            .OrderBy(table => table.Capacity)
            .ThenBy(table => table.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TimeOnly>> GetAvailableTimesAsync(
        int restaurantId,
        int tableId,
        DateOnly date,
        int guests,
        CancellationToken cancellationToken = default)
    {
        var table = await _dbContext.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(
                table =>
                    table.Id == tableId &&
                    table.RestaurantId == restaurantId,
                cancellationToken);

        if (table is null)
        {
            throw new KeyNotFoundException(
                "Selected table does not exist.");
        }

        if (!table.IsActive)
        {
            return [];
        }

        if (table.Capacity < guests)
        {
            return [];
        }

        var context = await GetDayContextAsync(
            restaurantId,
            date,
            cancellationToken);

        var serviceStartsAtUtc = ConvertLocalToUtc(
            context.OpeningLocal,
            context.TimeZone);

        var serviceEndsAtUtc = ConvertLocalToUtc(
            context.ClosingLocal,
            context.TimeZone);

        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.TableId == tableId &&
                BlockingStatuses.Contains(reservation.Status) &&
                reservation.StartsAtUtc < serviceEndsAtUtc &&
                reservation.TableAvailableAtUtc >
                    serviceStartsAtUtc)
            .ToListAsync(cancellationToken);

        var tableBlocks = await _dbContext.TableBlocks
            .AsNoTracking()
            .Where(tableBlock =>
                tableBlock.TableId == tableId &&
                tableBlock.CancelledAtUtc == null &&
                tableBlock.StartsAtUtc < serviceEndsAtUtc &&
                tableBlock.EndsAtUtc > serviceStartsAtUtc)
            .ToListAsync(cancellationToken);

        var availableTimes = new List<TimeOnly>();

        var localStart = context.OpeningLocal;

        while (localStart < context.ClosingLocal)
        {
            var validationError = ValidateLocalStart(
                context,
                localStart,
                DateTime.UtcNow);

            if (validationError is null)
            {
                var window = BuildWindow(
                    context,
                    localStart);

                var hasReservationConflict =
                    reservations.Any(reservation =>
                        reservation.StartsAtUtc <
                            window.TableAvailableAtUtc &&
                        reservation.TableAvailableAtUtc >
                            window.StartsAtUtc);

                var hasBlockConflict =
                    tableBlocks.Any(tableBlock =>
                        tableBlock.StartsAtUtc <
                            window.TableAvailableAtUtc &&
                        tableBlock.EndsAtUtc >
                            window.StartsAtUtc);

                if (!hasReservationConflict &&
                    !hasBlockConflict)
                {
                    availableTimes.Add(
                        TimeOnly.FromDateTime(localStart));
                }
            }

            localStart = localStart.AddMinutes(
                context.Settings.SlotIntervalMinutes);
        }

        return availableTimes;
    }

    private async Task<ReservationDayContext>
        GetDayContextAsync(
            int restaurantId,
            DateOnly date,
            CancellationToken cancellationToken)
    {
        var restaurant = await _dbContext.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                restaurant => restaurant.Id == restaurantId,
                cancellationToken);

        if (restaurant is null)
        {
            throw new KeyNotFoundException(
                "Restaurant does not exist.");
        }

        if (!restaurant.IsActive)
        {
            throw new ReservationValidationException(
                "Restaurant is currently inactive.");
        }

        var settings = await _dbContext.RestaurantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                settings =>
                    settings.RestaurantId == restaurantId,
                cancellationToken);

        if (settings is null)
        {
            throw new InvalidOperationException(
                "Restaurant settings are missing.");
        }

        ValidateSettings(settings);

        var openingHours = await _dbContext.OpeningHours
            .AsNoTracking()
            .FirstOrDefaultAsync(
                openingHours =>
                    openingHours.RestaurantId ==
                        restaurantId &&
                    openingHours.DayOfWeek ==
                        date.DayOfWeek,
                cancellationToken);

        if (openingHours is null ||
            openingHours.IsClosed)
        {
            throw new ReservationValidationException(
                "Restaurant is closed on the selected date.");
        }

        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                restaurant.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new InvalidOperationException(
                $"Unknown restaurant time zone: " +
                $"{restaurant.TimeZoneId}");
        }

        var openingLocal = DateTime.SpecifyKind(
            date.ToDateTime(openingHours.OpeningTime),
            DateTimeKind.Unspecified);

        var closingLocal = DateTime.SpecifyKind(
            date.ToDateTime(openingHours.ClosingTime),
            DateTimeKind.Unspecified);

        // Например: 18:00–02:00.
        if (closingLocal <= openingLocal)
        {
            closingLocal = closingLocal.AddDays(1);
        }

        return new ReservationDayContext(
            restaurant,
            settings,
            openingHours,
            timeZone,
            openingLocal,
            closingLocal);
    }

    private static string? ValidateLocalStart(
        ReservationDayContext context,
        DateTime localStart,
        DateTime utcNow)
    {
        if (context.TimeZone.IsInvalidTime(localStart))
        {
            return "Selected local time does not exist " +
                   "because of daylight saving time.";
        }

        var minutesFromOpening =
            (int)(localStart -
                  context.OpeningLocal).TotalMinutes;

        if (minutesFromOpening < 0 ||
            minutesFromOpening %
                context.Settings.SlotIntervalMinutes != 0)
        {
            return "Selected time is not a valid " +
                   "reservation slot.";
        }

        var localEnd = localStart.AddMinutes(
            context.Settings
                .DefaultReservationDurationMinutes);

        if (localStart < context.OpeningLocal ||
            localEnd > context.ClosingLocal)
        {
            return "Reservation is outside restaurant " +
                   "opening hours.";
        }

        var startsAtUtc = ConvertLocalToUtc(
            localStart,
            context.TimeZone);

        var earliestAllowedUtc = utcNow.AddMinutes(
            context.Settings.MinimumAdvanceMinutes);

        if (startsAtUtc < earliestAllowedUtc)
        {
            return "Selected time is too soon or is " +
                   "already in the past.";
        }

        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(
            utcNow,
            context.TimeZone);

        var maximumDate = DateOnly
            .FromDateTime(nowLocal)
            .AddDays(
                context.Settings.MaximumAdvanceDays);

        if (DateOnly.FromDateTime(localStart) >
            maximumDate)
        {
            return "Selected date is too far in advance.";
        }

        return null;
    }

    private static ReservationWindow BuildWindow(
        ReservationDayContext context,
        DateTime localStart)
    {
        var localEnd = localStart.AddMinutes(
            context.Settings
                .DefaultReservationDurationMinutes);

        var localTableAvailable = localEnd.AddMinutes(
            context.Settings.TurnoverBufferMinutes);

        return new ReservationWindow(
            ConvertLocalToUtc(
                localStart,
                context.TimeZone),
            ConvertLocalToUtc(
                localEnd,
                context.TimeZone),
            ConvertLocalToUtc(
                localTableAvailable,
                context.TimeZone));
    }

    private static DateTime ConvertLocalToUtc(
        DateTime localDateTime,
        TimeZoneInfo timeZone)
    {
        var unspecifiedDateTime =
            DateTime.SpecifyKind(
                localDateTime,
                DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(
            unspecifiedDateTime,
            timeZone);
    }

    private static void ValidateSettings(
        RestaurantSettings settings)
    {
        if (settings.DefaultReservationDurationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Reservation duration must be greater than 0.");
        }

        if (settings.SlotIntervalMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Slot interval must be greater than 0.");
        }

        if (settings.TurnoverBufferMinutes < 0 ||
            settings.MinimumAdvanceMinutes < 0 ||
            settings.MaximumAdvanceDays < 0)
        {
            throw new InvalidOperationException(
                "Restaurant settings contain " +
                "negative values.");
        }
    }

    public async Task<List<AvailableTimeOption>>
    GetAvailableTimesOverviewAsync(
        int restaurantId,
        DateOnly date,
        int guests,
        CancellationToken cancellationToken = default)
    {
        if (guests < 1)
        {
            throw new ReservationValidationException(
                "Guest count must be at least 1.");
        }

        var context = await GetDayContextAsync(
            restaurantId,
            date,
            cancellationToken);

        var tableIds = await _dbContext.Tables
            .AsNoTracking()
            .Where(table =>
                table.RestaurantId == restaurantId &&
                table.IsActive &&
                table.Capacity >= guests)
            .Select(table => table.Id)
            .ToListAsync(cancellationToken);

        if (tableIds.Count == 0)
        {
            return [];
        }

        var serviceStartsAtUtc = ConvertLocalToUtc(
            context.OpeningLocal,
            context.TimeZone);

        var serviceEndsAtUtc = ConvertLocalToUtc(
            context.ClosingLocal,
            context.TimeZone);

        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                tableIds.Contains(reservation.TableId) &&
                BlockingStatuses.Contains(reservation.Status) &&
                reservation.StartsAtUtc < serviceEndsAtUtc &&
                reservation.TableAvailableAtUtc >
                    serviceStartsAtUtc)
            .ToListAsync(cancellationToken);

        var tableBlocks = await _dbContext.TableBlocks
            .AsNoTracking()
            .Where(tableBlock =>
                tableIds.Contains(tableBlock.TableId) &&
                tableBlock.CancelledAtUtc == null &&
                tableBlock.StartsAtUtc < serviceEndsAtUtc &&
                tableBlock.EndsAtUtc > serviceStartsAtUtc)
            .ToListAsync(cancellationToken);

        var reservationsByTable = reservations
            .GroupBy(reservation => reservation.TableId)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        var blocksByTable = tableBlocks
            .GroupBy(tableBlock => tableBlock.TableId)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());

        var availableTimes = new List<AvailableTimeOption>();
        var localStart = context.OpeningLocal;
        var utcNow = DateTime.UtcNow;

        while (localStart < context.ClosingLocal)
        {
            var validationError = ValidateLocalStart(
                context,
                localStart,
                utcNow);

            if (validationError is null)
            {
                var window = BuildWindow(
                    context,
                    localStart);

                var availableTableCount = 0;

                foreach (var tableId in tableIds)
                {
                    var hasReservationConflict =
                        reservationsByTable.TryGetValue(
                            tableId,
                            out var tableReservations) &&
                        tableReservations.Any(reservation =>
                            reservation.StartsAtUtc <
                                window.TableAvailableAtUtc &&
                            reservation.TableAvailableAtUtc >
                                window.StartsAtUtc);

                    var hasBlockConflict =
                        blocksByTable.TryGetValue(
                            tableId,
                            out var tableBlockList) &&
                        tableBlockList.Any(tableBlock =>
                            tableBlock.StartsAtUtc <
                                window.TableAvailableAtUtc &&
                            tableBlock.EndsAtUtc >
                                window.StartsAtUtc);

                    if (!hasReservationConflict &&
                        !hasBlockConflict)
                    {
                        availableTableCount++;
                    }
                }

                if (availableTableCount > 0)
                {
                    availableTimes.Add(
                        new AvailableTimeOption(
                            TimeOnly.FromDateTime(localStart),
                            availableTableCount));
                }
            }

            localStart = localStart.AddMinutes(
                context.Settings.SlotIntervalMinutes);
        }

        return availableTimes;
    }

    private sealed record ReservationDayContext(
        Restaurant Restaurant,
        RestaurantSettings Settings,
        OpeningHours OpeningHours,
        TimeZoneInfo TimeZone,
        DateTime OpeningLocal,
        DateTime ClosingLocal);
}

