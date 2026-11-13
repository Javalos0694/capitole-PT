using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.EventHandlers;
using Bookings.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BookingsDbContext _db;
    private readonly IPublisher _publisher;

    public UnitOfWork(BookingsDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = _db.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException(
                "booking.concurrency_conflict",
                "La reserva fue modificada por otra operación. Intenta confirmar de nuevo.");
        }
    }
}
