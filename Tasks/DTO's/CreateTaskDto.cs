using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.DTO_s;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100)]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Due date is required")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "BoardId is required")]
    public int BoardId { get; set; }

    public Guid? ParentTaskId { get; set; } 
}
