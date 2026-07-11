using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Models;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("tables")]
public class TablesController : ControllerBase
{
    private static readonly List<RestaurantTable> Tables = new()
    {
        new RestaurantTable
        {
            Id = 1,
            Name = "T1",
            Capacity = 2,
            Zone = "Main Hall",
            XPosition = 100,
            YPosition = 150,
            IsActive = true
        },
        new RestaurantTable
        {
            Id = 2,
            Name = "T2",
            Capacity = 4,
            Zone = "Main Hall",
            XPosition = 250,
            YPosition = 150,
            IsActive = true
        }
    };

    [HttpGet]
    public ActionResult<List<RestaurantTable>> GetAll()
    {
        return Ok(Tables);
    }

    [HttpGet("{id:int}")]
    public ActionResult<RestaurantTable> GetById(int id)
    {
        var table = Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpPost]
    public ActionResult<RestaurantTable> Create(RestaurantTable table)
    {
        var nextId = Tables.Count == 0 ? 1 : Tables.Max(t => t.Id) + 1;

        table.Id = nextId;
        Tables.Add(table);

        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, RestaurantTable updatedTable)
    {
        var table = Tables.FirstOrDefault(t => t.Id == id);

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
        var table = Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        Tables.Remove(table);

        return NoContent();
    }
}
