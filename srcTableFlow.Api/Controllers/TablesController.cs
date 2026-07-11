using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Models;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("tables")]
public class TablesController : ControllerBase
{

    [HttpGet]
    public ActionResult<List<RestaurantTable>> GetAll()
    {
        return Ok(InMemoryStore.Tables);
    }

    [HttpGet("{id:int}")]
    public ActionResult<RestaurantTable> GetById(int id)
    {
        var table = InMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpGet("available")]
    public ActionResult<RestaurantTable> GetAvailableTables([FromQuery] DateOnly date, [FromQuery] TimeOnly time, [FromQuery] int guests)
    {
        var bookedTableIds = InMemoryStore.Reservations.Where(r =>
            r.ReservationDate == date &&
            r.ReservationTime == time &&
            r.Status != ReservationStatus.Cancelled).Select(r => r.TableId).ToList();

        var availableTables = InMemoryStore.Tables.Where(t =>
                t.IsActive &&
                t.Capacity >= guests &&
                !bookedTableIds.Contains(t.Id)).ToList();

        return Ok(availableTables);
    }

    [HttpPost]
    public ActionResult<RestaurantTable> Create(RestaurantTable table)
    {
        var nextId = InMemoryStore.Tables.Count == 0 ? 1 : InMemoryStore.Tables.Max(t => t.Id) + 1;

        table.Id = nextId;
        InMemoryStore.Tables.Add(table);

        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, RestaurantTable updatedTable)
    {
        var table = InMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        table.Name = updatedTable.Name;
        table.Capacity = updatedTable.Capacity;
        table.Zone = updatedTable.Zone;
        table.XPosition = updatedTable.XPosition;
        table.YPosition = updatedTable.YPosition;
        table.IsActive = updatedTable.IsActive;

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var table = InMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        InMemoryStore.Tables.Remove(table);

        return NoContent();
    }
}
