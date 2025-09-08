namespace shared.Events.Payloads;

public class TaskCreatedPayload
{
    public Guid TaskId { get; set; }
    public int BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
}