using Bookings.Application.Abstractions;
using MediatR;

namespace Bookings.Application.Bookings.Queries;

public sealed record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto?>;

public sealed class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingReadStore _readStore;

    public GetBookingByIdHandler(IBookingReadStore readStore) => _readStore = readStore;

    public Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken) =>
        _readStore.GetByIdAsync(request.Id, cancellationToken);
}
