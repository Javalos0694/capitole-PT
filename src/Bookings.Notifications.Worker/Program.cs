using Bookings.Contracts;
using Bookings.Notifications.Worker.Consumers;
using Bookings.Notifications.Worker.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Notifications")
    ?? throw new InvalidOperationException("Connection string 'Notifications' is missing.");

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(connectionString));

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingConfirmedConsumer>();
    x.AddConsumer<BookingCancelledConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("notifications-confirmed", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            e.PublishFaults = true;
        });

        cfg.ReceiveEndpoint("notifications-cancelled", e =>
        {
            e.ConfigureConsumer<BookingCancelledConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            e.PublishFaults = true;
        });
    });
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await host.RunAsync();
