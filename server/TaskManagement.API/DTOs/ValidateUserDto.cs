using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

public class ValidateUserDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
