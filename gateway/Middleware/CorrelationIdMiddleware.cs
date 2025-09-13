namespace gateway.Middleware;

public class CorrelationIdMiddleware(RequestDelegate _next , ILogger<CorrelationIdMiddleware> _logger)
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-Id";
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = correlationId;
        }

        var correlationIdValue = correlationId.ToString();
        context.Items[HeaderName] = correlationIdValue;

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationIdValue
               }))
        {
            await _next(context);
        }
    }
}
