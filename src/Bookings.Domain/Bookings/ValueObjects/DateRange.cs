using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }

    private DateRange()
    {
    }

    private DateRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
            throw new DomainException("booking.invalid_range", "La fecha de inicio debe ser anterior a la de fin.");

        return new DateRange(start.ToUniversalTime(), end.ToUniversalTime());
    }

    // Half-open [Start, End): adjacent ranges do not overlap.
    public bool Overlaps(DateRange other) =>
        Start < other.End && other.Start < End;

    public bool Equals(DateRange? other) =>
        other is not null && Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => obj is DateRange other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Start, End);
}
