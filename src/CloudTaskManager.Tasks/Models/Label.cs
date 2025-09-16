using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Label
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; } // Hex color code

    // Many-to-many with tasks
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}
