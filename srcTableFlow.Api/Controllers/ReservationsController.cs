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
        return Ok(IsMemoryStore.Reservations);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetById(int id)
    {
        var reservation = IsMemoryStore.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult<Reservation> Create(Reservation reservation)
    {
        var table = IsMemoryStore.Tables.FirstOrDefault(t => t.Id == reservation.TableID);

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

        var alreadyBooked = IsMemoryStore.Reservations.Any(r =>
            r.TableID == reservation.TableID &&
            r.ReservationDate == reservation.ReservationDate &&
            r.ReservationTime == reservation.ReservationTime &&
            r.Status != "Cancelled");

        if (alreadyBooked)
        {
            return Conflict("This table is already booked for the selected date and time.");
        }

        var nextId = IsMemoryStore.Reservations.Count == 0
            ? 1
            : IsMemoryStore.Reservations.Max(r => r.Id) + 1;

        reservation.Id = nextId;
        reservation.Status = "Confirmed";
        reservation.CreatedAtUtc = DateTime.UtcNow;

        IsMemoryStore.Reservations.Add(reservation);

        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpPatch("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var reservation = IsMemoryStore.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = "Cancelled";

        return NoContent();
    }
}