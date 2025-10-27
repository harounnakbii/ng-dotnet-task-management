using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registerDto.Name,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered successfully", user.Email);

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToDto);
    }

    public async Task<UserValidationResult> ValidateUserAsync(ValidateUserDto validateDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Name == validateDto.Username || u.Email == validateDto.Username) && u.IsActive);

        if (user == null)
        {
            return new UserValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        // Verify password
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(validateDto.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return new UserValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid username or password"
            };
        }

        return new UserValidationResult
        {
            IsValid = true,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return !await _context.Users.AnyAsync(u => u.Name.ToLower() == username.ToLower());
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            RegistrationDate = user.RegistrationDate
        };
    }
}