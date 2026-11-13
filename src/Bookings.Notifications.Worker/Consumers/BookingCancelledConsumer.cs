using Bookings.Contracts;
using Bookings.Notifications.Worker.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Notifications.Worker.Consumers;

public sealed class BookingCancelledConsumer : IConsumer<BookingCancelledV1>
{
    private readonly NotificationsDbContext _db;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(NotificationsDbContext db, ILogger<BookingCancelledConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCancelledV1> context)
    {
        var message = context.Message;

        if (await _db.Inbox.AnyAsync(x => x.EventId == message.EventId, context.CancellationToken))
        {
            _logger.LogInformation("Skipping duplicate event {EventId}", message.EventId);
            return;
        }

        _db.Inbox.Add(new InboxMessage
        {
            EventId = message.EventId,
            Consumer = nameof(BookingCancelledConsumer),
            ProcessedAtUtc = DateTimeOffset.UtcNow
        });

        _db.Notifications.Add(new NotificationRecord
        {
            Id = Guid.NewGuid(),
            EventId = message.EventId,
            CorrelationId = message.CorrelationId,
            BookingId = message.BookingId,
            Type = "BookingCancelled",
            CustomerEmail = message.CustomerEmail,
            Reason = message.Reason,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Notification registered for cancelled booking {BookingId}. CorrelationId={CorrelationId} EventId={EventId}",
            message.BookingId,
            message.CorrelationId,
            message.EventId);
    }
}
