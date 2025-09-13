namespace shared.Correlation;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
    void SetCorrelationId(string correlationId);
}