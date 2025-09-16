using CloudTaskManager.Notifications.Events;
using CloudTaskManager.Notifications.Hub;
using CloudTaskManager.Notifications.Middleware;
using CloudTaskManager.Shared.Correlation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClientPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5150") // Blazor client
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});
builder.Services.AddSignalR();
builder.Services.AddHostedService<RabbitMqEventSubscriber>();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("BlazorClientPolicy");
app.MapHub<NotificationHub>("/hub/notifications");
app.MapHealthChecks("/health");
app.UseCorrelationId();
app.Run();