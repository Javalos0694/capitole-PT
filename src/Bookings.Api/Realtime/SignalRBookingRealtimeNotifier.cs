using Bookings.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Bookings.Api.Realtime;

public sealed class SignalRBookingRealtimeNotifier : IBookingRealtimeNotifier
{
    private readonly IHubContext<BookingsHub> _hub;

    public SignalRBookingRealtimeNotifier(IHubContext<BookingsHub> hub) => _hub = hub;

    public Task NotifyChangedAsync(
        Guid bookingId,
        string status,
        string changeType,
        CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(
            "BookingChanged",
            new
            {
                bookingId,
                status,
                changeType
            },
            cancellationToken);
}
