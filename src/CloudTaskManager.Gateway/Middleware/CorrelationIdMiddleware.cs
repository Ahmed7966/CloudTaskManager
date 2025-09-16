namespace CloudTaskManager.Gateway.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next , ILogger<CorrelationIdMiddleware> logger)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = correlationId;
        }

        var correlationIdValue = correlationId.ToString();
        context.Items[HeaderName] = correlationIdValue;

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationIdValue
               }))
        {
            await next(context);
        }
    }
}
