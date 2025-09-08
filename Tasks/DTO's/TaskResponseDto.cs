using CloudTaskManager.Models;

namespace CloudTaskManager.DTO_s;

public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Status Status { get; set; }
    public string? AssignedToUserId { get; set; }
}