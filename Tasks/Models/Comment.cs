using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
