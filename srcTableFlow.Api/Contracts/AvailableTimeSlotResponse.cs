namespace TableFlow.Api.Contracts;

public class AvailableTimeSlotResponse
{
    public TimeOnly Time { get; set; }

    public int AvailableTableCount { get; set; }
}