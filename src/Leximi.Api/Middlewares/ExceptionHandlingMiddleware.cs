using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Leximi.Api.Middlewares;

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            _logger.LogError("==== CONCURRENCY EXCEPTION DETAILS ====");
            foreach (var entry in ex.Entries)
            {
                var idVal = entry.Property("Id")?.CurrentValue;
                _logger.LogError($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}, Id: {idVal}");
                
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    _logger.LogError("Entity NO LONGER EXISTS in the database (deleted by someone else).");
                }
                else
                {
                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        var original = entry.OriginalValues[property];
                        var database = databaseValues[property];
                        var current = entry.CurrentValues[property];

                        if (!Equals(original, database))
                        {
                            _logger.LogError($"Property '{property.Name}' mismatch: Original={original}, Database={database}, Proposed Current={current}");
                        }
                    }
                }
            }
            _logger.LogError("=======================================");
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            ArgumentException => HttpStatusCode.BadRequest,
            DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            Status = context.Response.StatusCode,
            Message = statusCode == HttpStatusCode.InternalServerError 
                ? "Internal Server Error. Please contact support." 
                : exception.Message,
            Detail = exception.Message 
        });

        return context.Response.WriteAsync(result);
    }
}
