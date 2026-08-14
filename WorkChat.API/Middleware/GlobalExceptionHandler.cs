using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkChat.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Nao autorizado"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito ao salvar os dados"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        logger.LogError(exception, "Erro nao tratado. TraceId: {TraceId}", context.TraceIdentifier);
        var problema = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == 500 ? "Ocorreu um erro inesperado." : exception.Message,
            Instance = context.Request.Path
        };
        problema.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problema, ct);
        return true;
    }
}
