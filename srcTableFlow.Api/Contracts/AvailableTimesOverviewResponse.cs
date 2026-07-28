namespace TableFlow.Api.Contracts;

public class AvailableTimesOverviewResponse
{
    public DateOnly Date { get; set; }

    public int Guests { get; set; }

    public List<AvailableTimeSlotResponse> AvailableTimes
    { get; set; } = [];
}