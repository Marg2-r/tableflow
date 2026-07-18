using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Contracts;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;
using TableFlow.Api.Models;
using TableFlow.Api.Services;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("restaurants/{restaurantId:int}/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly TableFlowDbContext _dbContext;
    private readonly ReservationAvailabilityService
        _availabilityService;

    public ReservationsController(
        TableFlowDbContext dbContext,
        ReservationAvailabilityService availabilityService)
    {
        _dbContext = dbContext;
        _availabilityService = availabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservationResponse>>> GetAll(
        int restaurantId,
        CancellationToken cancellationToken)
    {
        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Table)
            .Where(reservation =>
                reservation.RestaurantId == restaurantId)
            .OrderBy(reservation => reservation.StartsAtUtc)
            .ToListAsync(cancellationToken);

        var response = reservations
            .Select(ToResponse)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(
        int restaurantId,
        int id,
        CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Table)
            .FirstOrDefaultAsync(
                reservation =>
                    reservation.Id == id &&
                    reservation.RestaurantId == restaurantId,
                cancellationToken);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(reservation));
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(
        int restaurantId,
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var table = await _dbContext.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(
                table =>
                    table.Id == request.TableId &&
                    table.RestaurantId == restaurantId,
                cancellationToken);

        if (table is null)
        {
            return BadRequest(
                "Selected table does not exist in this restaurant.");
        }

        if (!table.IsActive)
        {
            return BadRequest(
                "Selected table is currently inactive.");
        }

        if (request.GuestCount > table.Capacity)
        {
            return BadRequest(
                $"Selected table can hold only " +
                $"{table.Capacity} guests.");
        }

        ReservationWindow window;

        try
        {
            window = await _availabilityService.CreateWindowAsync(
                restaurantId,
                request.ReservationDate,
                request.ReservationTime,
                cancellationToken);
        }
        catch (ReservationValidationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }

        var isAvailable =
            await _availabilityService.IsTableAvailableAsync(
                request.TableId,
                window,
                cancellationToken);

        if (!isAvailable)
        {
            return Conflict(
                "Selected table is not available for this time.");
        }

        var utcNow = DateTime.UtcNow;

        var reservation = new Reservation
        {
            RestaurantId = restaurantId,
            TableId = request.TableId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            GuestCount = request.GuestCount,
            StartsAtUtc = window.StartsAtUtc,
            EndsAtUtc = window.EndsAtUtc,
            TableAvailableAtUtc =
                window.TableAvailableAtUtc,
            Status = ReservationStatus.Confirmed,
            Notes = request.Notes,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        _dbContext.Reservations.Add(reservation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                restaurantId,
                id = reservation.Id
            },
            ToResponse(reservation, table.Name));
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(
        int restaurantId,
        int id,
        CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations
            .Include(reservation => reservation.Table)
            .FirstOrDefaultAsync(
                reservation =>
                    reservation.Id == id &&
                    reservation.RestaurantId == restaurantId,
                cancellationToken);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status ==
            ReservationStatus.Cancelled)
        {
            return BadRequest(
                "Reservation is already cancelled.");
        }

        var utcNow = DateTime.UtcNow;

        reservation.Status =
            ReservationStatus.Cancelled;

        reservation.CancelledAtUtc = utcNow;
        reservation.UpdatedAtUtc = utcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(reservation));
    }

    private static ReservationResponse ToResponse(
        Reservation reservation)
    {
        return ToResponse(
            reservation,
            reservation.Table?.Name ?? "Unknown table");
    }

    private static ReservationResponse ToResponse(
        Reservation reservation,
        string tableName)
    {
        return new ReservationResponse
        {
            Id = reservation.Id,
            RestaurantId = reservation.RestaurantId,
            TableId = reservation.TableId,
            TableName = tableName,
            CustomerName = reservation.CustomerName,
            CustomerEmail = reservation.CustomerEmail,
            CustomerPhone = reservation.CustomerPhone,
            GuestCount = reservation.GuestCount,
            StartsAtUtc = reservation.StartsAtUtc,
            EndsAtUtc = reservation.EndsAtUtc,
            TableAvailableAtUtc =
                reservation.TableAvailableAtUtc,
            DurationMinutes = (int)(
                reservation.EndsAtUtc -
                reservation.StartsAtUtc
            ).TotalMinutes,
            TurnoverBufferMinutes = (int)(
                reservation.TableAvailableAtUtc -
                reservation.EndsAtUtc
            ).TotalMinutes,
            Status = reservation.Status,
            Notes = reservation.Notes,
            CreatedAtUtc = reservation.CreatedAtUtc,
            UpdatedAtUtc = reservation.UpdatedAtUtc,
            CancelledAtUtc = reservation.CancelledAtUtc
        };
    }
}