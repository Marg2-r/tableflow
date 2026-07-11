using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Data;
using TableFlow.Api.Models;
using TableFlow.Api.Contracts;
using TableFlow.Api.Enums;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Reservation>> GetAll()
    {
        var response = InMemoryStore.Reservations
        .Select(ToResponse)
        .ToList();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetById(int id)
    {
        var reservation = InMemoryStore.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(reservation));
    }

    [HttpPost]
    public ActionResult<ReservationResponse> Create(CreateReservationRequest request)
    {
        if (request.ReservationDate == default)
        {
            return BadRequest("Reservation date is required.");
        }

        if (request.ReservationTime == default)
        {
            return BadRequest("Reservation time is required.");
        }

        if (request.ReservationDate < DateOnly.FromDateTime(DateTime.Now))
        {
            return BadRequest("Reservation date cannot be in the past.");
        }

        var table = InMemoryStore.Tables.FirstOrDefault(t => t.Id == request.TableId);

        if (table is null)
        {
            return BadRequest("Selected table does not exist.");
        }

        if (!table.IsActive)
        {
            return BadRequest("Selected table is not available for reservations.");
        }

        if (request.GuestCount > table.Capacity)
        {
            return BadRequest("Guest count is greater than table capacity.");
        }

        var alreadyBooked = InMemoryStore.Reservations.Any(r =>
            r.TableId == request.TableId &&
            r.ReservationDate == request.ReservationDate &&
            r.ReservationTime == request.ReservationTime &&
            r.Status != ReservationStatus.Cancelled);

        if (alreadyBooked)
        {
            return Conflict("This table is already booked for the selected date and time.");
        }

        var nextId = InMemoryStore.Reservations.Count == 0
            ? 1
            : InMemoryStore.Reservations.Max(r => r.Id) + 1;

        var reservation = new Reservation
        {
            Id = nextId,
            TableId = request.TableId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ReservationDate = request.ReservationDate,
            ReservationTime = request.ReservationTime,
            GuestCount = request.GuestCount,
            Status = ReservationStatus.Confirmed,
            CreatedAtUtc = DateTime.UtcNow
        };

        InMemoryStore.Reservations.Add(reservation);

        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, ToResponse(reservation));
    }

    [HttpPatch("{id:int}/cancel")]
    public ActionResult<ReservationResponse> Cancel(int id)
    {
        var reservation = InMemoryStore.Reservations
            .FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return BadRequest("Reservation is already cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        return Ok(ToResponse(reservation));
    }

    private static ReservationResponse ToResponse(Reservation reservation)
    {
        var table = InMemoryStore.Tables
            .FirstOrDefault(t => t.Id == reservation.TableId);

        return new ReservationResponse
        {
            Id = reservation.Id,
            TableId = reservation.TableId,
            TableName = table?.Name ?? "Unknown table",
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