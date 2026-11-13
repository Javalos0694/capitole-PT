namespace Bookings.Domain.Bookings.ValueObjects;

public readonly record struct BookingId(Guid Value)
{
    public static BookingId New() => new(Guid.NewGuid());

    public static BookingId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BookingId cannot be empty.", nameof(value));

        return new BookingId(value);
    }

    public override string ToString() => Value.ToString();
}
