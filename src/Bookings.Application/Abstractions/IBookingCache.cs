using Bookings.Application.Bookings.Queries;

namespace Bookings.Application.Abstractions;

public interface IBookingCache
{
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetByIdAsync(BookingDto booking, CancellationToken cancellationToken = default);
    Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<BookingDto>?> GetListAsync(BookingListQuery query, CancellationToken cancellationToken = default);
    Task SetListAsync(BookingListQuery query, PagedResult<BookingDto> result, CancellationToken cancellationToken = default);
    Task InvalidateListsAsync(CancellationToken cancellationToken = default);
}
