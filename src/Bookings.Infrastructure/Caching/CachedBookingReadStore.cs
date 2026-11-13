using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Bookings.Infrastructure.Persistence;

namespace Bookings.Infrastructure.Caching;

public sealed class CachedBookingReadStore : IBookingReadStore
{
    private readonly BookingReadStore _inner;
    private readonly IBookingCache _cache;

    public CachedBookingReadStore(BookingReadStore inner, IBookingCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetByIdAsync(id, cancellationToken);
        if (cached is not null)
            return cached;

        var booking = await _inner.GetByIdAsync(id, cancellationToken);
        if (booking is not null)
            await _cache.SetByIdAsync(booking, cancellationToken);

        return booking;
    }

    public async Task<PagedResult<BookingDto>> ListAsync(
        BookingListQuery query,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetListAsync(query, cancellationToken);
        if (cached is not null)
            return cached;

        var result = await _inner.ListAsync(query, cancellationToken);
        await _cache.SetListAsync(query, result, cancellationToken);
        return result;
    }
}
