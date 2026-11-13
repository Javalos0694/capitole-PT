using System.Text.Json;
using Bookings.Application.Abstractions;
using Bookings.Domain.Bookings.Events;
using MediatR;

namespace Bookings.Application.Bookings.EventHandlers;

public sealed class BookingConfirmedDomainHandler : INotificationHandler<DomainEventNotification<BookingConfirmed>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ICorrelationContext _correlation;

    public BookingConfirmedDomainHandler(IOutboxWriter outbox, ICorrelationContext correlation)
    {
        _outbox = outbox;
        _correlation = correlation;
    }

    public Task Handle(DomainEventNotification<BookingConfirmed> notification, CancellationToken cancellationToken)
    {
        var e = notification.DomainEvent;
        var payload = JsonSerializer.Serialize(new
        {
            e.EventId,
            e.OccurredAtUtc,
            CorrelationId = _correlation.CorrelationId,
            e.BookingId,
            e.ResourceId,
            e.Start,
            e.End,
            e.CustomerEmail
        });

        return _outbox.EnqueueAsync(
            "BookingConfirmedV1",
            payload,
            _correlation.CorrelationId,
            cancellationToken);
    }
}

public sealed class BookingCancelledDomainHandler : INotificationHandler<DomainEventNotification<BookingCancelled>>
{
    private readonly IOutboxWriter _outbox;
    private readonly ICorrelationContext _correlation;

    public BookingCancelledDomainHandler(IOutboxWriter outbox, ICorrelationContext correlation)
    {
        _outbox = outbox;
        _correlation = correlation;
    }

    public Task Handle(DomainEventNotification<BookingCancelled> notification, CancellationToken cancellationToken)
    {
        var e = notification.DomainEvent;
        var payload = JsonSerializer.Serialize(new
        {
            e.EventId,
            e.OccurredAtUtc,
            CorrelationId = _correlation.CorrelationId,
            e.BookingId,
            e.ResourceId,
            e.Start,
            e.End,
            e.CustomerEmail,
            e.Reason
        });

        return _outbox.EnqueueAsync(
            "BookingCancelledV1",
            payload,
            _correlation.CorrelationId,
            cancellationToken);
    }
}

public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : class;
