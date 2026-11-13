using Microsoft.EntityFrameworkCore;

namespace Bookings.Notifications.Worker.Persistence;

public sealed class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();
    public DbSet<InboxMessage> Inbox => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRecord>(b =>
        {
            b.ToTable("notifications");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(100).IsRequired();
            b.Property(x => x.CustomerEmail).HasMaxLength(256).IsRequired();
            b.HasIndex(x => x.EventId).IsUnique();
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("inbox_messages");
            b.HasKey(x => x.EventId);
            b.Property(x => x.Consumer).HasMaxLength(200).IsRequired();
        });
    }
}

public sealed class NotificationRecord
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid BookingId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public string Consumer { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAtUtc { get; set; }
}
