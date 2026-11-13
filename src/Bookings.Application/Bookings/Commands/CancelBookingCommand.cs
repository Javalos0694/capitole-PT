using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentValidation;
using MediatR;

namespace Bookings.Application.Bookings.Commands;

public sealed record CancelBookingCommand(Guid BookingId, string? Reason) : IRequest<BookingDto>;

public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}

public sealed class CancelBookingHandler : IRequestHandler<CancelBookingCommand, BookingDto>
{
    private readonly IBookingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IBookingCache _cache;
    private readonly IBookingRealtimeNotifier _realtime;

    public CancelBookingHandler(
        IBookingRepository repository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IBookingCache cache,
        IBookingRealtimeNotifier realtime)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _cache = cache;
        _realtime = realtime;
    }

    public async Task<BookingDto> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(BookingId.From(request.BookingId), cancellationToken)
            ?? throw new NotFoundException($"No se encontró la reserva {request.BookingId}.");

        booking.Cancel(request.Reason, _clock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = BookingMapper.ToDto(booking);
        await _cache.RemoveByIdAsync(dto.Id, cancellationToken);
        await _cache.InvalidateListsAsync(cancellationToken);
        await _realtime.NotifyChangedAsync(dto.Id, dto.Status, "cancelled", cancellationToken);
        return dto;
    }
}
