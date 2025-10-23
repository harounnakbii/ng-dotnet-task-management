using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
}