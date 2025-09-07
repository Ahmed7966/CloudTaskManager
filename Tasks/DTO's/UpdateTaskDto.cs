using System.ComponentModel.DataAnnotations;
using CloudTaskManager.Models;

namespace CloudTaskManager.DTO_s;

public class UpdateTaskDto
{
    [Required(ErrorMessage = "Task Id is required")]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Status? Status { get; set; }

    public int? BoardId { get; set; }
    public Guid? ParentTaskId { get; set; } 
}