namespace CloudTaskManager.Shared.Correlation;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
    void SetCorrelationId(string correlationId);
}