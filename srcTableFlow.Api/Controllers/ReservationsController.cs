using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Contracts;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;
using TableFlow.Api.Models;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private readonly TableFlowDbContext _dbContext;

    public ReservationsController(TableFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservationResponse>>> GetAll()
    {
        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Table)
            .OrderBy(reservation => reservation.ReservationDate)
            .ThenBy(reservation => reservation.ReservationTime)
            .ToListAsync();

        var response = reservations
            .Select(ToResponse)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id)
    {
        var reservation = await _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Table)
            .FirstOrDefaultAsync(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(reservation));
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request)
    {
        var table = await _dbContext.Tables
            .FirstOrDefaultAsync(table => table.Id == request.TableId);

        if (table is null)
        {
            return BadRequest("Selected table does not exist.");
        }

        if (!table.IsActive)
        {
            return BadRequest("Selected table is inactive.");
        }

        if (request.GuestCount > table.Capacity)
        {
            return BadRequest(
                $"Selected table can hold only {table.Capacity} guests.");
        }

        var isAlreadyReserved = await _dbContext.Reservations
            .AnyAsync(reservation =>
                reservation.TableId == request.TableId &&
                reservation.ReservationDate == request.ReservationDate &&
                reservation.ReservationTime == request.ReservationTime &&
                reservation.Status != ReservationStatus.Cancelled);

        if (isAlreadyReserved)
        {
            return Conflict(
                "Selected table is already reserved for this date and time.");
        }

        var reservation = new Reservation
        {
            TableId = request.TableId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ReservationDate = request.ReservationDate,
            ReservationTime = request.ReservationTime,
            GuestCount = request.GuestCount,
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow,
            Table = table
        };

        _dbContext.Reservations.Add(reservation);

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            ToResponse(reservation));
    }


    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(int id)
    {
        var reservation = await _dbContext.Reservations
            .Include(reservation => reservation.Table)
            .FirstOrDefaultAsync(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return BadRequest("Reservation is already cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(reservation));
    }

    private static ReservationResponse ToResponse(Reservation reservation)
    {
        return new ReservationResponse
        {
            Id = reservation.Id,
            TableId = reservation.TableId,
            TableName = reservation.Table?.Name ?? "Unknown table",
            CustomerName = reservation.CustomerName,
            CustomerEmail = reservation.CustomerEmail,
            CustomerPhone = reservation.CustomerPhone,
            ReservationDate = reservation.ReservationDate,
            ReservationTime = reservation.ReservationTime,
            GuestCount = reservation.GuestCount,
            Status = reservation.Status,
            CreatedAtUtc = reservation.CreatedAtUtc
        };
    }
}