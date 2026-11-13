using Bookings.Application.Abstractions;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public sealed class BookingRepository : IBookingRepository
{
    private readonly BookingsDbContext _db;

    public BookingRepository(BookingsDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(BookingId id, CancellationToken cancellationToken = default) =>
        _db.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        await _db.Bookings.AddAsync(booking, cancellationToken);

    public Task<bool> AnyAsync(Specification<Booking> specification, CancellationToken cancellationToken = default) =>
        _db.Bookings.AnyAsync(specification.ToExpression(), cancellationToken);
}
