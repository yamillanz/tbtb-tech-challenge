using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TbtbChallenge.Api.Exceptions;

namespace TbtbChallenge.Api.Handlers;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validation)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Solicitud inválida",
                Detail = validation.Message,
                Status = StatusCodes.Status400BadRequest
            };
            problemDetails.Extensions["errors"] = new Dictionary<string, string>
            {
                [validation.Field] = validation.Message
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }

        if (exception is KeyNotFoundException notFound)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Recurso no encontrado",
                Detail = notFound.Message,
                Status = StatusCodes.Status404NotFound
            };

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }

        return false;
    }
}
