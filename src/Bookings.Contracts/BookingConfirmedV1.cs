namespace Bookings.Contracts;

public sealed record BookingConfirmedV1(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CorrelationId,
    Guid BookingId,
    string ResourceId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string CustomerEmail);
