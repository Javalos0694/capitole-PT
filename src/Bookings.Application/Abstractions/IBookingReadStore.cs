using Bookings.Application.Bookings.Queries;

namespace Bookings.Application.Abstractions;

public interface IBookingReadStore
{
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDto>> ListAsync(
        BookingListQuery query,
        CancellationToken cancellationToken = default);
}
