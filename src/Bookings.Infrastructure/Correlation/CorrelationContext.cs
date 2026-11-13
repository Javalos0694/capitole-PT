using Bookings.Application.Abstractions;

namespace Bookings.Infrastructure.Correlation;

public sealed class CorrelationContext : ICorrelationContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}
