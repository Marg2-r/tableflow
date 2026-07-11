using Microsoft.AspNetCore.Mvc;
using TableFlow.Api.Models;
using TableFlow.Api.Data;

namespace TableFlow.Api.Controllers;

[ApiController]
[Route("tables")]
public class TablesController : ControllerBase
{

    [HttpGet]
    public ActionResult<List<RestaurantTable>> GetAll()
    {
        return Ok(IsMemoryStore.Tables);
    }

    [HttpGet("{id:int}")]
    public ActionResult<RestaurantTable> GetById(int id)
    {
        var table = IsMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpPost]
    public ActionResult<RestaurantTable> Create(RestaurantTable table)
    {
        var nextId = IsMemoryStore.Tables.Count == 0 ? 1 : IsMemoryStore.Tables.Max(t => t.Id) + 1;

        table.Id = nextId;
        IsMemoryStore.Tables.Add(table);

        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, RestaurantTable updatedTable)
    {
        var table = IsMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

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
        var table = IsMemoryStore.Tables.FirstOrDefault(t => t.Id == id);

        if (table is null)
        {
            return NotFound();
        }

        IsMemoryStore.Tables.Remove(table);

        return NoContent();
    }
}
