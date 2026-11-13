namespace Bookings.Domain.Bookings.ValueObjects;

public readonly record struct ResourceId(string Value)
{
    public static ResourceId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ResourceId is required.", nameof(value));

        return new ResourceId(value.Trim());
    }

    public override string ToString() => Value;
}
