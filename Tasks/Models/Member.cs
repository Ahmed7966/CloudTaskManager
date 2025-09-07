using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public required string Role { get; set; } = "User";

    // Relationships
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
}