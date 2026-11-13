using System.Text.RegularExpressions;
using Bookings.Domain.Common;

namespace Bookings.Domain.Bookings.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex Pattern = EmailRegex();

    public string Value { get; private set; } = null!;

    private Email()
    {
    }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("booking.invalid_email", "El correo es obligatorio.");

        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
            throw new DomainException("booking.invalid_email", "El formato del correo no es válido.");

        return new Email(normalized);
    }

    public bool Equals(Email? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Email other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
