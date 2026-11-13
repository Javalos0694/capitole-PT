namespace Bookings.Application.Abstractions;

public interface IOutboxWriter
{
    Task EnqueueAsync(string type, string payload, Guid correlationId, CancellationToken cancellationToken = default);
}
