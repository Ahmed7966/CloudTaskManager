using System.Text;
using System.Text.Json;
using CloudTaskManager.Message;
using CloudTaskManager.Models;
using RabbitMQ.Client;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "cloudtask.events";
    private readonly ConnectionFactory _factory;

    public RabbitMqEventPublisher(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("RabbitMQ:Host")!;
        _factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await DisposeAsync();
        
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection == null || !_connection.IsOpen || _channel == null || !_channel.IsOpen)
        {
            await InitializeAsync();
        }
    }

    private async Task PublishAsync(string routingKey, object payload)
    {
        await EnsureConnectedAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel!.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body);
    }

    public Task PublishTaskCreated(TaskItem task) =>
        PublishAsync("task.created", new { task.Id, task.Title, task.BoardId, task.AssignedToUserId });

    public Task PublishTaskUpdated(TaskItem task) =>
        PublishAsync("task.updated", new { task.Id, task.Title, task.Status, task.UpdatedAt });

    public Task PublishTaskDeleted(Guid taskId) =>
        PublishAsync("task.deleted", new { Id = taskId });

    public Task PublishReminderDue(Reminder reminder, string? userId)
    {
        return PublishAsync("reminder.due", new
        {
            reminder.Id,
            reminder.TaskItemId,
            reminder.ReminderTime,
            UserId = userId
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        _connection?.Dispose();
    }
}
