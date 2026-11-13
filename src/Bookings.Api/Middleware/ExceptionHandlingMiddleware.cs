using System.Text.Json;
using Bookings.Application.Abstractions;
using Bookings.Domain.Common;
using FluentValidation;

namespace Bookings.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title, code, errors) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Validación fallida",
                "validation.failed",
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => ToCamel(g.Key), g => g.Select(e => e.ErrorMessage).ToArray())),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                "not_found",
                (Dictionary<string, string[]>?)null),
            DomainException domain when domain.Code == "booking.overlap" => (
                StatusCodes.Status409Conflict,
                domain.Message,
                domain.Code,
                null),
            DomainException domain when domain.Code == "booking.invalid_transition" => (
                StatusCodes.Status422UnprocessableEntity,
                domain.Message,
                domain.Code,
                null),
            DomainException domain => (
                StatusCodes.Status400BadRequest,
                domain.Message,
                domain.Code,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado",
                "server.error",
                null)
        };

        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.com/{status}",
            ["title"] = title,
            ["status"] = status,
            ["detail"] = exception.Message,
            ["code"] = code
        };

        if (errors is not null)
            problem["errors"] = errors;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static string ToCamel(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}
