using Bookings.Api.Middleware;
using Bookings.Api.Realtime;
using Bookings.Application;
using Bookings.Application.Abstractions;
using Bookings.Infrastructure;
using Bookings.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<IBookingRealtimeNotifier, SignalRBookingRealtimeNotifier>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:3000"]));
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("frontend");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<BookingsHub>("/hubs/bookings");

await DbSeeder.MigrateAndSeedAsync(app.Services);

app.Run();

public partial class Program;
