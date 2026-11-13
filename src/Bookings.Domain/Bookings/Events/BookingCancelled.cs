using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.Events;

public sealed record BookingCancelled(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid BookingId,
    string ResourceId,
    string CustomerEmail,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Reason) : IDomainEvent;
