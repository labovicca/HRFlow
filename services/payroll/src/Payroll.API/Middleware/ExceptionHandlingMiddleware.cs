using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Payroll.API.Middleware;

/// <summary>
/// Globalni middleware koji hvata sve neobrađene exception-e i vraća
/// konzistentan JSON error response. Zamenjuje try-catch blokove u kontrolerima.
/// </summary>
public class ExceptionHandlingMiddleware
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
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validacija nije uspela.",
                Errors = validationEx.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()
            },
            KeyNotFoundException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = exception.Message
            },
            InvalidOperationException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = exception.Message
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Nemate pristup ovom resursu."
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Došlo je do interne greške servera."
            }
        };

        response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Errors { get; set; }
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
