using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TitanFitness.Application.Common.Exceptions;

namespace TitanFitness.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException exception)
        {
            await HandleValidationExceptionAsync(context, exception);
        }
        catch (NotFoundException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message);
        }
        catch (ConflictException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.");
        }
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => ToCamelCase(error.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error"
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}