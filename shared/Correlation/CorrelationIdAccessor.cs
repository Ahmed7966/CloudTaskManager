namespace shared.Correlation;

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId { get; private set; } = Guid.NewGuid().ToString();

    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}