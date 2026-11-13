using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.Specifications;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentValidation;
using MediatR;

namespace Bookings.Application.Bookings.Commands;

public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest<BookingDto>;

public sealed class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
{
    public ConfirmBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
    }
}

public sealed class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, BookingDto>
{
    private readonly IBookingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IBookingCache _cache;
    private readonly IBookingRealtimeNotifier _realtime;

    public ConfirmBookingHandler(
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

    public async Task<BookingDto> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(BookingId.From(request.BookingId), cancellationToken)
            ?? throw new NotFoundException($"No se encontró la reserva {request.BookingId}.");

        var overlap = new ConfirmedOverlappingBookingSpec(booking.ResourceId, booking.Range, booking.Id);
        if (await _repository.AnyAsync(overlap, cancellationToken))
            throw new DomainException("booking.overlap", "Ya existe una reserva confirmada que se solapa con este recurso y horario.");

        booking.Confirm(_clock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = BookingMapper.ToDto(booking);
        await _cache.RemoveByIdAsync(dto.Id, cancellationToken);
        await _cache.InvalidateListsAsync(cancellationToken);
        await _realtime.NotifyChangedAsync(dto.Id, dto.Status, "confirmed", cancellationToken);
        return dto;
    }
}
