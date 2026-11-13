using Bookings.Infrastructure.Messaging;
using Bookings.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Bookings.Api.IntegrationTests;

public sealed class BookingsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("bookings")
        .WithUsername("bookings")
        .WithPassword("bookings")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Bookings", _postgres.GetConnectionString());
        builder.UseSetting("RabbitMq:Host", "localhost");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<BookingsDbContext>));
            services.RemoveAll(typeof(BookingsDbContext));
            services.AddDbContext<BookingsDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("MassTransit", StringComparison.Ordinal) == true
                            || d.ImplementationType?.FullName?.Contains("MassTransit", StringComparison.Ordinal) == true)
                .ToList();
            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.RemoveAll(typeof(IHostedService));

            services.AddMassTransitTestHarness();
            services.AddHostedService<OutboxPublisherService>();
        });
    }
}
