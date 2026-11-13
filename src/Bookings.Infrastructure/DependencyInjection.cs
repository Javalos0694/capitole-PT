using Bookings.Application.Abstractions;
using Bookings.Domain.Common;
using Bookings.Infrastructure.Caching;
using Bookings.Infrastructure.Correlation;
using Bookings.Infrastructure.Messaging;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Persistence.Outbox;
using Bookings.Infrastructure.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Bookings")
            ?? throw new InvalidOperationException("Connection string 'Bookings' is missing.");

        services.AddDbContext<BookingsDbContext>(options =>
            options.UseNpgsql(connectionString));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "pt-capitole:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<BookingReadStore>();
        services.AddScoped<IBookingReadStore, CachedBookingReadStore>();
        services.AddScoped<IBookingCache, RedisBookingCache>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<CorrelationContext>();
        services.AddScoped<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());

        var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitUser = configuration["RabbitMq:Username"] ?? "guest";
        var rabbitPass = configuration["RabbitMq:Password"] ?? "guest";

        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });
            });
        });

        services.AddHostedService<OutboxPublisherService>();

        var healthChecks = services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres");

        if (!string.IsNullOrWhiteSpace(redisConnection))
            healthChecks.AddRedis(redisConnection, name: "redis");

        return services;
    }
}
