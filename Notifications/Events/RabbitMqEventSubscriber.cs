using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Notifications.Hub;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notifications.Events;
public class RabbitMqEventSubscriber : BackgroundService
{
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IHubContext<NotificationHub> _hub;

    private const string ExchangeName = "cloudtask.events";

    public RabbitMqEventSubscriber(ILogger<RabbitMqEventSubscriber> logger, IConfiguration config , IHubContext<NotificationHub>  notificationHub)
    {
        _hub = notificationHub;
        _logger = logger;
        _config = config;
        _ = InitRabbitMqAsync();
    }

    private async Task InitRabbitMqAsync()
    {
        var connectionString = _config.GetValue<string>("RabbitMQ:Host");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("RabbitMQ connection string is missing in appsettings.json");

        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);

        var queueName = $"notification-service-{Guid.NewGuid()}";
        await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: true, autoDelete: true);

        
        await _channel.QueueBindAsync(queueName, ExchangeName, "task.*");
        await _channel.QueueBindAsync(queueName, ExchangeName, "reminder.*");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnEventReceivedAsync;

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

        _logger.LogInformation("✅ RabbitMQ subscriber initialized (queue {Queue})", queueName);
    }

    private async Task OnEventReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;
            var payload = JsonSerializer.Deserialize<JsonElement>(message);

            if (routingKey == "task.created" && payload.TryGetProperty("BoardId", out var boardId))
            {
                await _hub.Clients.Group($"board:{boardId}")
                    .SendAsync("taskCreated", payload);
            }
            else if (routingKey == "task.updated" && payload.TryGetProperty("BoardId", out var boardId2))
            {
                await _hub.Clients.Group($"board:{boardId2}")
                    .SendAsync("taskUpdated", payload);
            }
            else if (routingKey == "reminder.due" && payload.TryGetProperty("UserId", out var userId))
            {
                await _hub.Clients.Group($"user:{userId.GetString()}")
                    .SendAsync("reminderDue", payload);
            }

            _logger.LogInformation("📩 Event {RoutingKey}: {Message}", routingKey, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event");
        }
    }




    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitRabbitMqAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken); // keep alive
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
