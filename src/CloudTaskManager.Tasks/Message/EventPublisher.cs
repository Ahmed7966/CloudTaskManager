using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace CloudTaskManager.Message;

public class EventPublisher(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetValue<string>("RabbitMQ:Host")!;

    public async Task Publish<T>(string eventName, T message)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_connectionString)
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: "cloudtask.events", type: ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: "cloudtask.events",
            routingKey: eventName,
            body: body
        );
    }
}