using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.ValueObjects;

public sealed class Customer : IEquatable<Customer>
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;

    private Customer()
    {
    }

    private Customer(string name, Email email)
    {
        Name = name;
        Email = email;
    }

    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("booking.invalid_customer", "El nombre del cliente es obligatorio.");

        var trimmed = name.Trim();
        if (trimmed.Length > 100)
            throw new DomainException("booking.invalid_customer", "El nombre del cliente no puede superar 100 caracteres.");

        return new Customer(trimmed, Email.Create(email));
    }

    public bool Equals(Customer? other) =>
        other is not null
        && string.Equals(Name, other.Name, StringComparison.Ordinal)
        && Email.Equals(other.Email);

    public override bool Equals(object? obj) => obj is Customer other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Name, Email);
}
