namespace Bookings.Application.Bookings.Queries;

public sealed record BookingDto(
    Guid Id,
    string ResourceId,
    string CustomerName,
    string CustomerEmail,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record BookingListQuery(
    string? ResourceId,
    string? CustomerName,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 20,
    string? Sort = "created_desc");
