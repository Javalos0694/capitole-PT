using Bookings.Application.Abstractions;
using Bookings.Domain.Common;

namespace Bookings.Infrastructure.Persistence.Outbox;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly BookingsDbContext _db;
    private readonly IClock _clock;

    public OutboxWriter(BookingsDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task EnqueueAsync(
        string type,
        string payload,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await _db.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            CorrelationId = correlationId,
            OccurredAtUtc = _clock.UtcNow
        }, cancellationToken);
    }
}
