using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
}