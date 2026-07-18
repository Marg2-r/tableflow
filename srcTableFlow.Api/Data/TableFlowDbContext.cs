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

    public DbSet<Restaurant> Restaurants =>
        Set<Restaurant>();

    public DbSet<RestaurantSettings> RestaurantSettings =>
        Set<RestaurantSettings>();

    public DbSet<OpeningHours> OpeningHours =>
        Set<OpeningHours>();

    public DbSet<RestaurantTable> Tables =>
        Set<RestaurantTable>();

    public DbSet<Reservation> Reservations =>
        Set<Reservation>();

    public DbSet<TableBlock> TableBlocks =>
        Set<TableBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRestaurant(modelBuilder);
        ConfigureRestaurantSettings(modelBuilder);
        ConfigureOpeningHours(modelBuilder);
        ConfigureRestaurantTable(modelBuilder);
        ConfigureReservation(modelBuilder);
        ConfigureTableBlock(modelBuilder);
        AddInitialData(modelBuilder);
    }

    private static void ConfigureRestaurant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.Property(restaurant => restaurant.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(restaurant => restaurant.TimeZoneId)
                .HasMaxLength(100)
                .IsRequired();
        });
    }

    private static void ConfigureRestaurantSettings(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestaurantSettings>(entity =>
        {
            entity.HasKey(settings => settings.RestaurantId);

            entity.HasOne(settings => settings.Restaurant)
                .WithOne()
                .HasForeignKey<RestaurantSettings>(
                    settings => settings.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOpeningHours(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpeningHours>(entity =>
        {
            entity.HasIndex(openingHours => new
            {
                openingHours.RestaurantId,
                openingHours.DayOfWeek
            })
                .IsUnique();

            entity.HasOne(openingHours => openingHours.Restaurant)
                .WithMany()
                .HasForeignKey(openingHours =>
                    openingHours.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRestaurantTable(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestaurantTable>(entity =>
        {
            entity.Property(table => table.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(table => table.Zone)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(table => table.RestaurantId);

            entity.HasOne(table => table.Restaurant)
                .WithMany()
                .HasForeignKey(table => table.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReservation(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(reservation => reservation.CustomerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(reservation => reservation.CustomerEmail)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(reservation => reservation.CustomerPhone)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(reservation => reservation.Notes)
                .HasMaxLength(500);

            entity.HasIndex(reservation => new
            {
                reservation.RestaurantId,
                reservation.StartsAtUtc
            });

            entity.HasIndex(reservation => new
            {
                reservation.TableId,
                reservation.StartsAtUtc,
                reservation.TableAvailableAtUtc
            });

            entity.HasIndex(reservation => reservation.Status);

            entity.HasOne(reservation => reservation.Restaurant)
                .WithMany()
                .HasForeignKey(reservation =>
                    reservation.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(reservation => reservation.Table)
                .WithMany()
                .HasForeignKey(reservation => reservation.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTableBlock(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableBlock>(entity =>
        {
            entity.Property(tableBlock => tableBlock.Reason)
                .HasMaxLength(300)
                .IsRequired();

            entity.HasIndex(tableBlock => new
            {
                tableBlock.TableId,
                tableBlock.StartsAtUtc,
                tableBlock.EndsAtUtc
            });

            entity.HasOne(tableBlock => tableBlock.Table)
                .WithMany()
                .HasForeignKey(tableBlock => tableBlock.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void AddInitialData(ModelBuilder modelBuilder)
    {
        const int restaurantId = 1;

        modelBuilder.Entity<Restaurant>().HasData(
            new Restaurant
            {
                Id = restaurantId,
                Name = "TableFlow Demo Restaurant",
                TimeZoneId = "Europe/Prague",
                IsActive = true,
                CreatedAtUtc = new DateTime(
                    2026,
                    7,
                    18,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc)
            });

        modelBuilder.Entity<RestaurantSettings>().HasData(
            new RestaurantSettings
            {
                RestaurantId = restaurantId,
                DefaultReservationDurationMinutes = 120,
                SlotIntervalMinutes = 15,
                TurnoverBufferMinutes = 0,
                MinimumAdvanceMinutes = 30,
                MaximumAdvanceDays = 90
            });

        modelBuilder.Entity<OpeningHours>().HasData(
            CreateOpeningHours(
                id: 1,
                restaurantId,
                DayOfWeek.Monday),
            CreateOpeningHours(
                id: 2,
                restaurantId,
                DayOfWeek.Tuesday),
            CreateOpeningHours(
                id: 3,
                restaurantId,
                DayOfWeek.Wednesday),
            CreateOpeningHours(
                id: 4,
                restaurantId,
                DayOfWeek.Thursday),
            CreateOpeningHours(
                id: 5,
                restaurantId,
                DayOfWeek.Friday),
            CreateOpeningHours(
                id: 6,
                restaurantId,
                DayOfWeek.Saturday),
            CreateOpeningHours(
                id: 7,
                restaurantId,
                DayOfWeek.Sunday));
    }

    private static OpeningHours CreateOpeningHours(
        int id,
        int restaurantId,
        DayOfWeek dayOfWeek)
    {
        return new OpeningHours
        {
            Id = id,
            RestaurantId = restaurantId,
            DayOfWeek = dayOfWeek,
            OpeningTime = new TimeOnly(12, 0),
            ClosingTime = new TimeOnly(23, 0),
            IsClosed = false
        };
    }
}