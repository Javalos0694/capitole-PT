using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.Events;

public sealed record BookingCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid BookingId,
    string ResourceId,
    string CustomerName,
    string CustomerEmail,
    DateTimeOffset Start,
    DateTimeOffset End) : IDomainEvent;
