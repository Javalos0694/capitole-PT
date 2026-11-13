using System.Linq.Expressions;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.Specifications;

public sealed class BookingListCriteria : Specification<Booking>
{
    private readonly ResourceId? _resourceId;
    private readonly string? _customerName;
    private readonly DateTimeOffset? _from;
    private readonly DateTimeOffset? _to;

    public BookingListCriteria(
        string? resourceId,
        string? customerName,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        _resourceId = string.IsNullOrWhiteSpace(resourceId) ? null : ResourceId.From(resourceId);
        _customerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim().ToLowerInvariant();
        _from = from?.ToUniversalTime();
        _to = to?.ToUniversalTime();
    }

    public override Expression<Func<Booking, bool>> ToExpression()
    {
        Expression<Func<Booking, bool>> expression = booking => true;

        if (_resourceId is { } resourceId)
            expression = expression.AndAlso(booking => booking.ResourceId == resourceId);

        if (_customerName is { } customerName)
            expression = expression.AndAlso(booking =>
                booking.Customer.Name.ToLower().Contains(customerName));

        if (_from is { } from)
            expression = expression.AndAlso(booking => booking.Range.End > from);

        if (_to is { } to)
            expression = expression.AndAlso(booking => booking.Range.Start < to);

        return expression;
    }
}
