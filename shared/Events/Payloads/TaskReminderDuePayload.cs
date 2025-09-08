namespace shared.Events.Payloads;

public class TaskReminderDuePayload
{
    public Guid ReminderId { get; set; }
    public Guid TaskId { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDate { get; set; }
}