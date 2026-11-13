namespace Bookings.Application.Abstractions;

public interface IBookingRealtimeNotifier
{
    Task NotifyChangedAsync(
        Guid bookingId,
        string status,
        string changeType,
        CancellationToken cancellationToken = default);
}
