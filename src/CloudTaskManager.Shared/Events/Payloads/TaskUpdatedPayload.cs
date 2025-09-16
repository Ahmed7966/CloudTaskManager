namespace CloudTaskManager.Shared.Events.Payloads;

public class TaskUpdatedPayload
{
    public Guid TaskId { get; set; }
    public int BoardId { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDate { get; set; }
}