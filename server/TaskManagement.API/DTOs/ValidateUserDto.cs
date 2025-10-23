using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

public class ValidateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
