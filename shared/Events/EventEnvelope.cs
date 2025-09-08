namespace shared.Events;

public class EventEnvelope<TPayload>
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public int SchemaVersion { get; set; } = 1;
    public string? CorrelationId { get; set; }
    public TPayload Payload { get; set; }

    public EventEnvelope(string eventType, TPayload payload, string? correlationId = null)
    {
        EventType = eventType;
        Payload = payload;
        CorrelationId = correlationId;
    }
}