using System.Text.Json;
using Bookings.Contracts;
using Bookings.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.Messaging;

public sealed class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Outbox publisher cycle failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var pending = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                await PublishMessageAsync(publishEndpoint, message, cancellationToken);
                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                _logger.LogWarning(ex, "Failed to publish outbox message {OutboxId}", message.Id);
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task PublishMessageAsync(
        IPublishEndpoint publishEndpoint,
        Persistence.Outbox.OutboxMessage message,
        CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case "BookingConfirmedV1":
            {
                var payload = JsonSerializer.Deserialize<BookingConfirmedV1>(message.Payload)
                    ?? throw new InvalidOperationException("Invalid BookingConfirmedV1 payload");
                await publishEndpoint.Publish(payload, cancellationToken);
                break;
            }
            case "BookingCancelledV1":
            {
                var payload = JsonSerializer.Deserialize<BookingCancelledV1>(message.Payload)
                    ?? throw new InvalidOperationException("Invalid BookingCancelledV1 payload");
                await publishEndpoint.Publish(payload, cancellationToken);
                break;
            }
            default:
                throw new InvalidOperationException($"Unknown outbox type {message.Type}");
        }
    }
}
