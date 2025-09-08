using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Reminder
{
    [Key]
    public int Id { get; set; }

    public DateTime ReminderTime { get; set; }
    public bool IsSent { get; set; } = false;
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}
