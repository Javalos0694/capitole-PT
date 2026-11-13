using Bookings.Application.Behaviors;
using Bookings.Application.Bookings.Commands;
using Bookings.Domain.Common;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace Bookings.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_blocks_invalid_command()
    {
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var behavior = new ValidationBehavior<CreateBookingCommand, object>(
            [new CreateBookingCommandValidator(clock)]);

        var command = new CreateBookingCommand(
            "",
            "",
            "bad-email",
            clock.UtcNow.AddHours(-1),
            clock.UtcNow.AddHours(-2));

        var act = () => behavior.Handle(command, () => Task.FromResult<object>(new()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}
