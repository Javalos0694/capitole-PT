using Bookings.Domain.Bookings.Events;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings;

public sealed class Booking : Entity
{
    public BookingId Id { get; private set; }
    public ResourceId ResourceId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateRange Range { get; private set; } = null!;
    public BookingStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public uint RowVersion { get; private set; }

    private Booking()
    {
    }

    public static Booking Create(
        ResourceId resourceId,
        Customer customer,
        DateRange range,
        IClock clock)
    {
        if (range.Start <= clock.UtcNow)
            throw new DomainException("booking.start_in_past", "La fecha de inicio debe ser futura.");

        var booking = new Booking
        {
            Id = BookingId.New(),
            ResourceId = resourceId,
            Customer = customer,
            Range = range,
            Status = BookingStatus.Pending,
            CreatedAtUtc = clock.UtcNow
        };

        booking.Raise(new BookingCreated(
            Guid.NewGuid(),
            clock.UtcNow,
            booking.Id.Value,
            booking.ResourceId.Value,
            booking.Customer.Name,
            booking.Customer.Email.Value,
            booking.Range.Start,
            booking.Range.End));

        return booking;
    }

    public void Confirm(IClock clock)
    {
        EnsureTransition(BookingStatus.Confirmed);

        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = clock.UtcNow;

        Raise(new BookingConfirmed(
            Guid.NewGuid(),
            clock.UtcNow,
            Id.Value,
            ResourceId.Value,
            Customer.Email.Value,
            Range.Start,
            Range.End));
    }

    public void Cancel(string? reason, IClock clock)
    {
        EnsureTransition(BookingStatus.Cancelled);

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = clock.UtcNow;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        Raise(new BookingCancelled(
            Guid.NewGuid(),
            clock.UtcNow,
            Id.Value,
            ResourceId.Value,
            Customer.Email.Value,
            Range.Start,
            Range.End,
            CancellationReason));
    }

    private void EnsureTransition(BookingStatus target)
    {
        var allowed = Status switch
        {
            BookingStatus.Pending => target is BookingStatus.Confirmed or BookingStatus.Cancelled,
            BookingStatus.Confirmed => target is BookingStatus.Cancelled,
            _ => false
        };

        if (!allowed)
            throw new DomainException(
                "booking.invalid_transition",
                $"No se puede pasar de {Status} a {target}.");
    }
}
