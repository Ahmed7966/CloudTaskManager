using Notifications.Events;
using Notifications.Hub;

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

var app = builder.Build();

app.UseCors("BlazorClientPolicy");
app.MapHub<NotificationHub>("/hub/notifications");

app.Run();