using Bookings.Application.Abstractions;
using MediatR;

namespace Bookings.Application.Bookings.Queries;

public sealed record GetBookingsQuery(
    string? ResourceId,
    string? CustomerName,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20,
    string? Sort = "created_desc") : IRequest<PagedResult<BookingDto>>;

public sealed class GetBookingsHandler : IRequestHandler<GetBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IBookingReadStore _readStore;

    public GetBookingsHandler(IBookingReadStore readStore) => _readStore = readStore;

    public Task<PagedResult<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        return _readStore.ListAsync(
            new BookingListQuery(
                request.ResourceId,
                request.CustomerName,
                request.From,
                request.To,
                page,
                pageSize,
                request.Sort),
            cancellationToken);
    }
}
