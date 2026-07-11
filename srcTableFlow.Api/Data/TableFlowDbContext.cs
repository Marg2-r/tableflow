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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reservation>()
            .HasOne(reservation => reservation.Table)
            .WithMany()
            .HasForeignKey(reservation => reservation.TableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}