using System.ComponentModel.DataAnnotations;

namespace CloudTaskManager.Identity.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage =
            "Password must have at least one uppercase letter, one lowercase letter, one number, and one special character.")]
    public required string Password { get; set; }
}