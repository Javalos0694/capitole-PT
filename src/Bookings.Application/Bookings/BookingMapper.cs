using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;

namespace Bookings.Application.Bookings;

public static class BookingMapper
{
    public static BookingDto ToDto(Booking booking) => new(
        booking.Id.Value,
        booking.ResourceId.Value,
        booking.Customer.Name,
        booking.Customer.Email.Value,
        booking.Range.Start,
        booking.Range.End,
        booking.Status.ToString(),
        booking.CreatedAtUtc,
        booking.ConfirmedAtUtc,
        booking.CancelledAtUtc,
        booking.CancellationReason);
}
