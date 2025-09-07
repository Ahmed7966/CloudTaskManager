using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Reminder
{
    [Key]
    public int Id { get; set; }

    public DateTime ReminderTime { get; set; }

    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}
