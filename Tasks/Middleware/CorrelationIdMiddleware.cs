using shared.Correlation;

namespace CloudTaskManager.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor accessor)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        accessor.SetCorrelationId(correlationId);
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }
}