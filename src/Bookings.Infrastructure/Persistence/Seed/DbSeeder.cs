using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BookingsDbContext>>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Bookings.AnyAsync())
            return;

        var tomorrow = clock.UtcNow.Date.AddDays(1).AddHours(9);
        var samples = new[]
        {
            Booking.Create(
                ResourceId.From("Room-A"),
                Customer.Create("Ana Pérez", "ana@example.com"),
                DateRange.Create(tomorrow, tomorrow.AddHours(1)),
                clock),
            Booking.Create(
                ResourceId.From("Room-B"),
                Customer.Create("Luis Gómez", "luis@example.com"),
                DateRange.Create(tomorrow.AddHours(2), tomorrow.AddHours(3)),
                clock),
            Booking.Create(
                ResourceId.From("Desk-12"),
                Customer.Create("Marta Ruiz", "marta@example.com"),
                DateRange.Create(tomorrow.AddDays(1), tomorrow.AddDays(1).AddHours(4)),
                clock)
        };

        foreach (var booking in samples)
            booking.ClearDomainEvents();

        db.Bookings.AddRange(samples);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample bookings", samples.Length);
    }
}
