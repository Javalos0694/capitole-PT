using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Bookings.Domain.Bookings.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public sealed class BookingReadStore : IBookingReadStore
{
    private readonly BookingsDbContext _db;

    public BookingReadStore(BookingsDbContext db) => _db = db;

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == BookingId.From(id), cancellationToken);

        return booking is null ? null : Map(booking);
    }

    public async Task<PagedResult<BookingDto>> ListAsync(
        BookingListQuery query,
        CancellationToken cancellationToken = default)
    {
        var criteria = new BookingListCriteria(query.ResourceId, query.CustomerName, query.From, query.To);
        var filtered = _db.Bookings.AsNoTracking().Where(criteria.ToExpression());

        var total = await filtered.CountAsync(cancellationToken);

        var ordered = (query.Sort?.ToLowerInvariant()) switch
        {
            "start" => filtered.OrderBy(x => x.Range.Start),
            "start_desc" => filtered.OrderByDescending(x => x.Range.Start),
            "created" => filtered.OrderBy(x => x.CreatedAtUtc),
            "created_desc" => filtered.OrderByDescending(x => x.CreatedAtUtc),
            _ => filtered.OrderByDescending(x => x.CreatedAtUtc)
        };

        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingDto>(items.Select(Map).ToList(), query.Page, query.PageSize, total);
    }

    private static BookingDto Map(Booking booking) => new(
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
