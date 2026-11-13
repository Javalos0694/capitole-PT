using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Commands;
using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentAssertions;
using Moq;

namespace Bookings.Application.Tests;

public class ConfirmBookingHandlerTests
{
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 11, 13, 8, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Confirm_without_overlap_succeeds()
    {
        var booking = CreatePending();
        var repo = new Mock<IBookingRepository>();
        var uow = new Mock<IUnitOfWork>();
        var cache = new Mock<IBookingCache>();

        repo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        repo.Setup(r => r.AnyAsync(It.IsAny<Specification<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new ConfirmBookingHandler(repo.Object, uow.Object, _clock, cache.Object);
        var result = await handler.Handle(new ConfirmBookingCommand(booking.Id.Value), CancellationToken.None);

        result.Status.Should().Be(nameof(BookingStatus.Confirmed));
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_with_overlap_throws()
    {
        var booking = CreatePending();
        var repo = new Mock<IBookingRepository>();
        var uow = new Mock<IUnitOfWork>();
        var cache = new Mock<IBookingCache>();

        repo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        repo.Setup(r => r.AnyAsync(It.IsAny<ConfirmedOverlappingBookingSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new ConfirmBookingHandler(repo.Object, uow.Object, _clock, cache.Object);
        var act = () => handler.Handle(new ConfirmBookingCommand(booking.Id.Value), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Code.Should().Be("booking.overlap");
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Booking CreatePending() =>
        Booking.Create(
            ResourceId.From("Room-A"),
            Customer.Create("Ana", "ana@example.com"),
            DateRange.Create(_clock.UtcNow.AddDays(1), _clock.UtcNow.AddDays(1).AddHours(1)),
            _clock);

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}
