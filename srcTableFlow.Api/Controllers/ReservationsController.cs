using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Data;
using TableFlow.Api.Models;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Reservation>> GetAll()
    {
        return Ok(InMemoryStore.Reservations);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetById(int id)
    {
        var reservation = InMemoryStore.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult<Reservation> Create(Reservation reservation)
    {
        var table = InMemoryStore.Tables.FirstOrDefault(t => t.Id == reservation.TableId);

        if (table is null)
        {
            return BadRequest("Selected table does not exist.");
        }

        if (!table.IsActive)
        {
            return BadRequest("Selected table is not available for reservations.");
        }

        if (reservation.GuestCount > table.Capacity)
        {
            return BadRequest("Guest count is greater than table capacity.");
        }

        var alreadyBooked = InMemoryStore.Reservations.Any(r =>
            r.TableId == reservation.TableId &&
            r.ReservationDate == reservation.ReservationDate &&
            r.ReservationTime == reservation.ReservationTime &&
            r.Status != "Cancelled");

        if (alreadyBooked)
        {
            return Conflict("This table is already booked for the selected date and time.");
        }

        var nextId = InMemoryStore.Reservations.Count == 0
            ? 1
            : InMemoryStore.Reservations.Max(r => r.Id) + 1;

        reservation.Id = nextId;
        reservation.Status = "Confirmed";
        reservation.CreatedAtUtc = DateTime.UtcNow;

        InMemoryStore.Reservations.Add(reservation);

        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPatch("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var reservation = InMemoryStore.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = "Cancelled";

        return NoContent();
    }
}