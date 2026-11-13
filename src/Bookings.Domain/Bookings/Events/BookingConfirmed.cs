using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.Events;

public sealed record BookingConfirmed(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid BookingId,
    string ResourceId,
    string CustomerEmail,
    DateTimeOffset Start,
    DateTimeOffset End) : IDomainEvent;
