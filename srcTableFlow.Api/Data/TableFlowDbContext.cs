using Microsoft.EntityFrameworkCore;
using TableFlow.Api.Models;

namespace TableFlow.Api.Data;

public class TableFlowDbContext : DbContext
{
    public TableFlowDbContext(
        DbContextOptions<TableFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<RestaurantTable> Tables =>
        Set<RestaurantTable>();

    public DbSet<Reservation> Reservations =>
        Set<Reservation>();
}