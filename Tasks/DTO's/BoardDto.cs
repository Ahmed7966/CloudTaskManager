using System.ComponentModel.DataAnnotations;
namespace CloudTaskManager.DTO_s;

public class BoardDto
{
    [Required(ErrorMessage = "Board name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}