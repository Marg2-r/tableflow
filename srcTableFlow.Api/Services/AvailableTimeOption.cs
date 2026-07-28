namespace TableFlow.Api.Services;

public sealed record AvailableTimeOption(
    TimeOnly Time,
    int AvailableTableCount);