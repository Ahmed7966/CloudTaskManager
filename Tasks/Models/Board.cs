using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Board
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Board name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Relationships
    public List<TaskItem> Tasks { get; set; } = new();
    public List<Member> Members { get; set; } = new();
}