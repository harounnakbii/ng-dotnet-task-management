using IdentityServer.DTOs;
using System.Text;
using System.Text.Json;

namespace IdentityServer.Services;

public class UserValidationService : IUserValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserValidationService> _logger;

    public UserValidationService(HttpClient httpClient, ILogger<UserValidationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserValidationResult?> ValidateUserAsync(string email, string password)
    {
        try
        {
            var validateDto = new
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(validateDto),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://localhost:5002/api/users/validate", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UserValidationResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user {Email}", email);
            return null;
        }
    }
}