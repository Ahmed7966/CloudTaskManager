using System.ComponentModel.DataAnnotations;
using CloudTaskManager.Models;

namespace CloudTaskManager.DTO_s;

public class UpdateBoardDto
{
    [Required(ErrorMessage = "Id is required")]
    public int Id { get; set; }
    public string? Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    
    public List<Guid>? TaskIds { get; set; } = [];
    public List<int>? MemberIds { get; set; } = [];
}