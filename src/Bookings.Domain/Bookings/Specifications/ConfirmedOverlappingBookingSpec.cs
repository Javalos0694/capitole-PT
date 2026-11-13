using System.Linq.Expressions;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.Specifications;

public sealed class ConfirmedOverlappingBookingSpec : Specification<Booking>
{
    private readonly ResourceId _resourceId;
    private readonly DateTimeOffset _start;
    private readonly DateTimeOffset _end;
    private readonly BookingId? _excludeId;

    public ConfirmedOverlappingBookingSpec(ResourceId resourceId, DateRange range, BookingId? excludeId = null)
    {
        _resourceId = resourceId;
        _start = range.Start;
        _end = range.End;
        _excludeId = excludeId;
    }

    public override Expression<Func<Booking, bool>> ToExpression()
    {
        var resourceId = _resourceId;
        var start = _start;
        var end = _end;

        if (_excludeId is { } excludeId)
        {
            return booking =>
                booking.Status == BookingStatus.Confirmed
                && booking.ResourceId == resourceId
                && booking.Id != excludeId
                && booking.Range.Start < end
                && start < booking.Range.End;
        }

        return booking =>
            booking.Status == BookingStatus.Confirmed
            && booking.ResourceId == resourceId
            && booking.Range.Start < end
            && start < booking.Range.End;
    }
}
