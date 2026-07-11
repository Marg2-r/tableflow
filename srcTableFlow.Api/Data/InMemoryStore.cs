using TableFlow.Api.Models;

namespace TableFlow.Api.Data;

public static class InMemoryStore
{
    public static List<RestaurantTable> Tables { get; } = new()
    {
        new RestaurantTable{
            Id = 1,
            Name = "T1",
            Capacity = 2,
            Zone = "Main Hall",
            XPosition = 100,
            YPosition = 150,
            IsActive = true
        },
        new RestaurantTable{
            Id = 2,
            Name = "T2",
            Capacity = 4,
            Zone = "Garden",
            XPosition = 256,
            YPosition = 32,
            IsActive = true
        }
    };

    public static List<Reservation> Reservations { get; } = new();





}
