namespace CloudTaskManager.Shared.Events.Payloads;

public class TaskDeletedPayload
{
    public Guid TaskId { get; set; }
    public int BoardId { get; set; }
}