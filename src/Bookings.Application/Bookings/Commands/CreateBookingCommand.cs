using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentValidation;
using MediatR;

namespace Bookings.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    string ResourceId,
    string CustomerName,
    string CustomerEmail,
    DateTimeOffset Start,
    DateTimeOffset End) : IRequest<BookingDto>;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator(IClock clock)
    {
        RuleFor(x => x.ResourceId).NotEmpty().WithMessage("El recurso es obligatorio.");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(100)
            .WithMessage("El nombre no puede superar 100 caracteres.");
        RuleFor(x => x.CustomerEmail).NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo no es válido.");
        RuleFor(x => x.Start).GreaterThan(clock.UtcNow).WithMessage("La fecha de inicio debe ser futura.");
        RuleFor(x => x.End).GreaterThan(x => x.Start).WithMessage("La fecha de fin debe ser posterior al inicio.");
    }
}

public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IBookingCache _cache;
    private readonly IBookingRealtimeNotifier _realtime;

    public CreateBookingHandler(
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

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = Booking.Create(
            ResourceId.From(request.ResourceId),
            Customer.Create(request.CustomerName, request.CustomerEmail),
            DateRange.Create(request.Start, request.End),
            _clock);

        await _repository.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = BookingMapper.ToDto(booking);
        await _cache.InvalidateListsAsync(cancellationToken);
        await _realtime.NotifyChangedAsync(dto.Id, dto.Status, "created", cancellationToken);
        return dto;
    }
}
