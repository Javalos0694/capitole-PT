namespace Bookings.Application.Abstractions;

public interface ICorrelationContext
{
    Guid CorrelationId { get; }
}
