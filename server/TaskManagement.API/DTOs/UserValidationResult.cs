namespace TaskManagement.API.DTOs;

public class UserValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
}