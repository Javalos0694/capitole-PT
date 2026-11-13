using Bookings.Infrastructure.Correlation;

namespace Bookings.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, CorrelationContext correlation)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values)
            && Guid.TryParse(values.FirstOrDefault(), out var incoming))
        {
            correlation.CorrelationId = incoming;
        }
        else
        {
            correlation.CorrelationId = Guid.NewGuid();
        }

        context.Response.Headers[HeaderName] = correlation.CorrelationId.ToString();
        using (context.RequestServices.GetRequiredService<ILoggerFactory>()
                   .CreateLogger("Correlation")
                   .BeginScope(new Dictionary<string, object>
                   {
                       ["CorrelationId"] = correlation.CorrelationId
                   }))
        {
            await _next(context);
        }
    }
}
