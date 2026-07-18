using System;
using System.Collections.Generic;
using System.Text;

namespace TableFlow.Api.Models;

public class Restaurant
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = "Europe/Prague";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
