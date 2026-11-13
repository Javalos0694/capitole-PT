using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentAssertions;

namespace Bookings.Domain.Tests;

public class BookingTests
{
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 11, 13, 8, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_starts_as_pending()
    {
        var booking = CreateSample();

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "BookingCreated");
    }

    [Fact]
    public void Confirm_from_pending_is_allowed()
    {
        var booking = CreateSample();
        booking.ClearDomainEvents();

        booking.Confirm(_clock);

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ConfirmedAtUtc.Should().Be(_clock.UtcNow);
        booking.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "BookingConfirmed");
    }

    [Fact]
    public void Cancel_from_confirmed_is_allowed()
    {
        var booking = CreateSample();
        booking.Confirm(_clock);
        booking.ClearDomainEvents();

        booking.Cancel("changed plans", _clock);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancellationReason.Should().Be("changed plans");
    }

    [Fact]
    public void Cancel_from_cancelled_fails()
    {
        var booking = CreateSample();
        booking.Cancel(null, _clock);

        var act = () => booking.Cancel("again", _clock);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("booking.invalid_transition");
    }

    [Fact]
    public void Create_rejects_start_in_the_past()
    {
        var act = () => Booking.Create(
            ResourceId.From("Room-A"),
            Customer.Create("Ana", "ana@example.com"),
            DateRange.Create(_clock.UtcNow.AddHours(-1), _clock.UtcNow.AddHours(1)),
            _clock);

        act.Should().Throw<DomainException>().Which.Code.Should().Be("booking.start_in_past");
    }

    private Booking CreateSample() =>
        Booking.Create(
            ResourceId.From("Room-A"),
            Customer.Create("Ana Pérez", "ana@example.com"),
            DateRange.Create(_clock.UtcNow.AddDays(1), _clock.UtcNow.AddDays(1).AddHours(1)),
            _clock);

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}
