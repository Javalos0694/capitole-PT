namespace Bookings.Domain.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
