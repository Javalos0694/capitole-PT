using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bookings.Contracts;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Api.IntegrationTests;

public class ConfirmBookingIntegrationTests : IClassFixture<BookingsApiFactory>
{
    private readonly BookingsApiFactory _factory;

    public ConfirmBookingIntegrationTests(BookingsApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Confirm_publishes_BookingConfirmedV1()
    {
        var client = _factory.CreateClient();
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var start = DateTimeOffset.UtcNow.AddDays(2);
            var createResponse = await client.PostAsJsonAsync("/api/bookings", new
            {
                resourceId = "Room-IT",
                customerName = "Integration User",
                customerEmail = "it@example.com",
                start,
                end = start.AddHours(1)
            });

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            using var createdDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
            var id = createdDoc.RootElement.GetProperty("id").GetGuid();

            var confirmResponse = await client.PostAsync($"/api/bookings/{id}/confirm", null);
            confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var published = await harness.Published.Any<BookingConfirmedV1>(
                x => x.Context.Message.BookingId == id,
                cts.Token);

            published.Should().BeTrue();
        }
        finally
        {
            await harness.Stop();
        }
    }
}
