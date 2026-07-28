using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Contracts;
using TableFlow.Api.Data;
using TableFlow.Api.Models;
using TableFlow.Api.Services;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("restaurants/{restaurantId:int}/tables")]
public class TablesController : ControllerBase
{
    private readonly TableFlowDbContext _dbContext;
    private readonly ReservationAvailabilityService
        _availabilityService;

    public TablesController(
        TableFlowDbContext dbContext,
        ReservationAvailabilityService availabilityService)
    {
        _dbContext = dbContext;
        _availabilityService = availabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RestaurantTable>>> GetAll(
        int restaurantId,
        CancellationToken cancellationToken)
    {
        var tables = await _dbContext.Tables
            .AsNoTracking()
            .Where(table =>
                table.RestaurantId == restaurantId)
            .OrderBy(table => table.Id)
            .ToListAsync(cancellationToken);

        return Ok(tables);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RestaurantTable>> GetById(
        int restaurantId,
        int id,
        CancellationToken cancellationToken)
    {
        var table = await _dbContext.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(
                table =>
                    table.Id == id &&
                    table.RestaurantId == restaurantId,
                cancellationToken);

        if (table is null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RestaurantTable>>>
        GetAvailable(
            int restaurantId,
            [FromQuery] DateOnly date,
            [FromQuery] TimeOnly time,
            [FromQuery] int guests,
            CancellationToken cancellationToken)
    {
        try
        {
            var tables =
                await _availabilityService
                    .GetAvailableTablesAsync(
                        restaurantId,
                        date,
                        time,
                        guests,
                        cancellationToken);

            return Ok(tables);
        }
        catch (ReservationValidationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpGet("available-times")]
    public async Task<ActionResult<AvailableTimesOverviewResponse>>
    GetAvailableTimesOverview(
        int restaurantId,
        [FromQuery] DateOnly date,
        [FromQuery] int guests,
        CancellationToken cancellationToken)
    {
        try
        {
            var availableTimes =
                await _availabilityService
                    .GetAvailableTimesOverviewAsync(
                        restaurantId,
                        date,
                        guests,
                        cancellationToken);

            var response = new AvailableTimesOverviewResponse
            {
                Date = date,
                Guests = guests,
                AvailableTimes = availableTimes
                    .Select(option =>
                        new AvailableTimeSlotResponse
                        {
                            Time = option.Time,
                            AvailableTableCount =
                                option.AvailableTableCount
                        })
                    .ToList()
            };

            return Ok(response);
        }
        catch (ReservationValidationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }


    [HttpGet("{tableId:int}/available-times")]
    public async Task<ActionResult<AvailableTimesResponse>>
        GetAvailableTimes(
            int restaurantId,
            int tableId,
            [FromQuery] DateOnly date,
            [FromQuery] int guests,
            CancellationToken cancellationToken)
    {
        try
        {
            var availableTimes =
                await _availabilityService
                    .GetAvailableTimesAsync(
                        restaurantId,
                        tableId,
                        date,
                        guests,
                        cancellationToken);

            return Ok(new AvailableTimesResponse
            {
                TableId = tableId,
                Date = date,
                AvailableTimes = availableTimes
            });
        }
        catch (ReservationValidationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantTable>> Create(
        int restaurantId,
        RestaurantTable table,
        CancellationToken cancellationToken)
    {
        var restaurantExists =
            await _dbContext.Restaurants.AnyAsync(
                restaurant =>
                    restaurant.Id == restaurantId,
                cancellationToken);

        if (!restaurantExists)
        {
            return NotFound("Restaurant does not exist.");
        }

        table.Id = 0;
        table.RestaurantId = restaurantId;

        _dbContext.Tables.Add(table);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                restaurantId,
                id = table.Id
            },
            table);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RestaurantTable>> Update(
        int restaurantId,
        int id,
        RestaurantTable updatedTable,
        CancellationToken cancellationToken)
    {
        var table = await _dbContext.Tables
            .FirstOrDefaultAsync(
                table =>
                    table.Id == id &&
                    table.RestaurantId == restaurantId,
                cancellationToken);

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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(table);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(
        int restaurantId,
        int id,
        CancellationToken cancellationToken)
    {
        var table = await _dbContext.Tables
            .FirstOrDefaultAsync(
                table =>
                    table.Id == id &&
                    table.RestaurantId == restaurantId,
                cancellationToken);

        if (table is null)
        {
            return NotFound();
        }

        // Не удаляем физически, чтобы не потерять
        // историю связанных резерваций.
        table.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}