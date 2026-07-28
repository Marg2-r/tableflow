using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Contracts;
using TableFlow.Api.Data;
using TableFlow.Api.Models;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("restaurants/{restaurantId:int}/management/settings")]
public class ManagementSettingsController : ControllerBase
{
    private readonly TableFlowDbContext _dbContext;

    public ManagementSettingsController(
        TableFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<RestaurantSettingsResponse>> Get(
        int restaurantId,
        CancellationToken cancellationToken)
    {
        var settings = await _dbContext.RestaurantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                settings =>
                    settings.RestaurantId == restaurantId,
                cancellationToken);

        if (settings is null)
        {
            return NotFound("Restaurant settings were not found.");
        }

        return Ok(ToResponse(settings));
    }

    [HttpPut]
    public async Task<ActionResult<RestaurantSettingsResponse>> Update(
        int restaurantId,
        UpdateRestaurantSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (60 % request.SlotIntervalMinutes != 0)
        {
            return BadRequest(
                "Slot interval must divide evenly into 60 minutes.");
        }

        if (request.DefaultReservationDurationMinutes <
            request.SlotIntervalMinutes)
        {
            return BadRequest(
                "Reservation duration cannot be shorter than the slot interval.");
        }

        if (request.DefaultReservationDurationMinutes %
            request.SlotIntervalMinutes != 0)
        {
            return BadRequest(
                "Reservation duration must be divisible by the slot interval.");
        }

        var settings = await _dbContext.RestaurantSettings
            .FirstOrDefaultAsync(
                settings =>
                    settings.RestaurantId == restaurantId,
                cancellationToken);

        if (settings is null)
        {
            return NotFound("Restaurant settings were not found.");
        }

        settings.DefaultReservationDurationMinutes =
            request.DefaultReservationDurationMinutes;

        settings.SlotIntervalMinutes =
            request.SlotIntervalMinutes;

        settings.TurnoverBufferMinutes =
            request.TurnoverBufferMinutes;

        settings.MinimumAdvanceMinutes =
            request.MinimumAdvanceMinutes;

        settings.MaximumAdvanceDays =
            request.MaximumAdvanceDays;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(settings));
    }

    private static RestaurantSettingsResponse ToResponse(
        RestaurantSettings settings)
    {
        return new RestaurantSettingsResponse
        {
            RestaurantId = settings.RestaurantId,

            DefaultReservationDurationMinutes =
                settings.DefaultReservationDurationMinutes,

            SlotIntervalMinutes =
                settings.SlotIntervalMinutes,

            TurnoverBufferMinutes =
                settings.TurnoverBufferMinutes,

            MinimumAdvanceMinutes =
                settings.MinimumAdvanceMinutes,

            MaximumAdvanceDays =
                settings.MaximumAdvanceDays
        };
    }
}