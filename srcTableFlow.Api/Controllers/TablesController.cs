using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Models;
using TableFlow.Api.Data;
using TableFlow.Api.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("tables")]
public class TablesController : ControllerBase
{
    private readonly TableFlowDbContext _dbContext;

    public TablesController(TableFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<RestaurantTable>>> GetAll()
    {
        var tables = await _dbContext.Tables
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .ToListAsync();

        return Ok(tables);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<List<RestaurantTable>>> GetById(int id)
    {
        var table = await _dbContext.Tables
            .AsNoTracking().FirstOrDefaultAsync(t=>t.Id == id);

        if(table is null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RestaurantTable>>> GetAvailable([FromQuery] DateOnly date, [FromQuery] TimeOnly time, [FromQuery] int guests)
    {
        var availableTables = await _dbContext.Tables
          .AsNoTracking()
          .Where(table =>
              table.IsActive &&
              table.Capacity >= guests &&
              !_dbContext.Reservations.Any(reservation =>
                  reservation.TableId == table.Id &&
                  reservation.ReservationDate == date &&
                  reservation.ReservationTime == time &&
                  reservation.Status != ReservationStatus.Cancelled))
          .OrderBy(table => table.Capacity)
          .ToListAsync();

        return Ok(availableTables);
    }


    [HttpPost]
    public async Task<ActionResult<RestaurantTable>> Create(
        RestaurantTable table)
    {
        table.Id = 0;

        _dbContext.Tables.Add(table);

        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = table.Id },
            table);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RestaurantTable>> Update(
        int id,
        RestaurantTable updatedTable)
    {
        var table = await _dbContext.Tables.FindAsync(id);

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

        await _dbContext.SaveChangesAsync();

        return Ok(table);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var table = await _dbContext.Tables.FindAsync(id);

        if (table is null)
        {
            return NotFound();
        }

        _dbContext.Tables.Remove(table);

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
