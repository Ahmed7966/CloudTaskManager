using System.Text;
using System.Text.Json;
using CloudTaskManager.Notifications.Hub;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CloudTaskManager.Notifications.Events;
public class RabbitMqEventSubscriber(
    ILogger<RabbitMqEventSubscriber> logger,
    IConfiguration config,
    IHubContext<NotificationHub> notificationHub)
    : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "cloudtask.events";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        logger.LogInformation("✅ RabbitMQ subscriber is started running.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitRabbitMqAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken); 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ RabbitMQ connection failed. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); 
            }
        }
    }

    private async Task InitRabbitMqAsync(CancellationToken stoppingToken)
    {
        var connectionString = config.GetValue<string>("RabbitMQ:Host");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("RabbitMQ connection string is missing in appsettings.json");

        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken:stoppingToken);

        await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        var queueName = $"notification-service-{Guid.NewGuid()}";
        await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: true, autoDelete: true, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(queueName, ExchangeName, "task.*", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, ExchangeName, "reminder.*", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnEventReceivedAsync;

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

        logger.LogInformation("✅ RabbitMQ subscriber initialized (queue {Queue})", queueName);

        _connection.ConnectionShutdownAsync += async (_, reason) =>
        {
            logger.LogWarning("⚠️ RabbitMQ connection closed: {Reason}. Reconnecting...", reason.ReplyText);
            await ReconnectAsync(stoppingToken);
        };
    }

    private async Task ReconnectAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        await InitRabbitMqAsync(stoppingToken);
    }

    private async Task OnEventReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;
            var payload = JsonSerializer.Deserialize<JsonElement>(message);

            var correlationId = ea.BasicProperties?.CorrelationId ?? Guid.NewGuid().ToString();
            
            if (routingKey == "task.created" && payload.TryGetProperty("BoardId", out var boardId))
            {
                await notificationHub.Clients.Group($"board:{boardId}")
                    .SendAsync("taskCreated", payload);

                logger.LogInformation("📩 TaskCreated event processed for Board {BoardId} [CorrelationId: {CorrelationId}]",
                    boardId,
                    correlationId);
            }
            else if (routingKey == "task.updated" && payload.TryGetProperty("BoardId", out var boardId2))
            {
                await notificationHub.Clients.Group($"board:{boardId2}")
                    .SendAsync("taskUpdated", payload);

                logger.LogInformation("📩 TaskUpdated event processed for Board {BoardId} [CorrelationId: {CorrelationId}]",
                    boardId2,
                    correlationId);
            }
            else if (routingKey == "reminder.due" && payload.TryGetProperty("UserId", out var userId))
            {
                await notificationHub.Clients.Group($"user:{userId.GetString()}")
                    .SendAsync("reminderDue", payload);

                logger.LogInformation("⏰ ReminderDue event processed for User {UserId} [CorrelationId: {CorrelationId}]",
                    userId.GetString(),
                    correlationId);
            }
            else
            {
                logger.LogWarning("⚠️ Unhandled event {RoutingKey}: {Message} [CorrelationId: {CorrelationId}]",
                    routingKey,
                    message,
                    correlationId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event");
        }
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
