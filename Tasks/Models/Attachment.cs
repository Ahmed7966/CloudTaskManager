using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Attachment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FileName { get; set; }

    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
