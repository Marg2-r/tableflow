namespace TableFlow.Api.Contracts;

public class AvailableTimesResponse
{
    public int TableId { get; set; }

    public DateOnly Date { get; set; }

    public List<TimeOnly> AvailableTimes { get; set; } = [];
}