using System.Text.Json;
using Bookings.Application.Abstractions;
using Bookings.Application.Bookings.Queries;
using Microsoft.Extensions.Caching.Distributed;

namespace Bookings.Infrastructure.Caching;

public sealed class RedisBookingCache : IBookingCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan DetailTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(1);

    private readonly IDistributedCache _cache;

    public RedisBookingCache(IDistributedCache cache) => _cache = cache;

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(DetailKey(id), cancellationToken);
        return bytes is null ? null : JsonSerializer.Deserialize<BookingDto>(bytes, JsonOptions);
    }

    public Task SetByIdAsync(BookingDto booking, CancellationToken cancellationToken = default) =>
        _cache.SetAsync(
            DetailKey(booking.Id),
            JsonSerializer.SerializeToUtf8Bytes(booking, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DetailTtl },
            cancellationToken);

    public Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(DetailKey(id), cancellationToken);

    public async Task<PagedResult<BookingDto>?> GetListAsync(
        BookingListQuery query,
        CancellationToken cancellationToken = default)
    {
        var version = await GetVersionAsync(cancellationToken);
        var bytes = await _cache.GetAsync(ListKey(version, query), cancellationToken);
        return bytes is null
            ? null
            : JsonSerializer.Deserialize<PagedResult<BookingDto>>(bytes, JsonOptions);
    }

    public async Task SetListAsync(
        BookingListQuery query,
        PagedResult<BookingDto> result,
        CancellationToken cancellationToken = default)
    {
        var version = await GetVersionAsync(cancellationToken);
        await _cache.SetAsync(
            ListKey(version, query),
            JsonSerializer.SerializeToUtf8Bytes(result, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ListTtl },
            cancellationToken);
    }

    public async Task InvalidateListsAsync(CancellationToken cancellationToken = default)
    {
        var current = await GetVersionAsync(cancellationToken);
        var next = (long.TryParse(current, out var value) ? value : 0) + 1;
        await _cache.SetStringAsync(
            VersionKey,
            next.ToString(),
            new DistributedCacheEntryOptions(),
            cancellationToken);
    }

    private async Task<string> GetVersionAsync(CancellationToken cancellationToken)
    {
        var version = await _cache.GetStringAsync(VersionKey, cancellationToken);
        return string.IsNullOrWhiteSpace(version) ? "0" : version;
    }

    private static string DetailKey(Guid id) => $"bookings:detail:{id:D}";

    private static string ListKey(string version, BookingListQuery query) =>
        $"bookings:list:v{version}:{query.ResourceId}|{query.CustomerName}|{query.From}|{query.To}|{query.Page}|{query.PageSize}|{query.Sort}";

    private const string VersionKey = "bookings:list:version";
}
