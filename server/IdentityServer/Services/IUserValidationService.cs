using IdentityServer.DTOs;

namespace IdentityServer.Services
{
    public interface IUserValidationService
    {
        Task<UserValidationResult?> ValidateUserAsync(string email, string password);
    }
}
