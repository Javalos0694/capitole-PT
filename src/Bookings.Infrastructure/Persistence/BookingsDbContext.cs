using Bookings.Domain.Bookings;
using Bookings.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public sealed class BookingsDbContext : DbContext
{
    public BookingsDbContext(DbContextOptions<BookingsDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
