using System.Text.Json;
using TikTokShop.Application.Common.Models;
using TikTokShop.Domain.Exceptions;
using AppValidationException = TikTokShop.Domain.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace TikTokShop.Api.Middlewares;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        ApiResponse<object> response;

        switch (exception)
        {
            case AppValidationException ve:
                statusCode = StatusCodes.Status400BadRequest;
                response = ApiResponse.Fail(ve.Message, ve.Errors.Count > 0 ? ve.Errors : null);
                break;

            case FluentValidationException fve:
                statusCode = StatusCodes.Status400BadRequest;
                response = ApiResponse.Fail(
                    "One or more validation errors occurred.",
                    fve.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                );
                break;

            case AppException ae:
                statusCode = ae.StatusCode;
                response = ApiResponse.Fail(ae.Message);
                break;

            default:
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                statusCode = StatusCodes.Status500InternalServerError;
                var message = env.IsProduction()
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message;
                response = ApiResponse.Fail(message);
                break;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
