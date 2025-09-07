using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class TaskItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Please enter the name of the task.")]
    [MaxLength(100)]
    public required string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Status Status { get; set; } = Status.Pending;

    public DateTime DueDate { get; set; }

    public string? AssignedToUserId { get; set; }

    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
    
    public Guid? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }

    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
    
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Reminder> Reminders { get; set; } = [];
}