using Bookings.Domain.Bookings.ValueObjects;
using Bookings.Domain.Common;
using FluentAssertions;

namespace Bookings.Domain.Tests;

public class DateRangeTests
{
    [Fact]
    public void Overlaps_detects_intersection()
    {
        var a = DateRange.Create(At(10), At(11));
        var b = DateRange.Create(At(10, 30), At(11, 30));

        a.Overlaps(b).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_allows_adjacency()
    {
        var a = DateRange.Create(At(10), At(11));
        var b = DateRange.Create(At(11), At(12));

        a.Overlaps(b).Should().BeFalse();
    }

    [Fact]
    public void Create_rejects_inverted_range()
    {
        var act = () => DateRange.Create(At(12), At(10));
        act.Should().Throw<DomainException>().Which.Code.Should().Be("booking.invalid_range");
    }

    private static DateTimeOffset At(int hour, int minute = 0) =>
        new(2026, 12, 1, hour, minute, 0, TimeSpan.Zero);
}
