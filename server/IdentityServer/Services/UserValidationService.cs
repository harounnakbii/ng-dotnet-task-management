using Duende.IdentityModel.Client;
using System.Text;
using System.Text.Json;

namespace IdentityServer.Services;

public class UserValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserValidationService> _logger;
    private readonly IConfiguration _configuration;
    private string? _cachedAccessToken;
    private DateTime _tokenExpirationTime;

    public UserValidationService(
        HttpClient httpClient,
        ILogger<UserValidationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Valide les credentials d'un utilisateur auprès de la Task API
    /// </summary>
    public async Task<UserValidationResult?> ValidateUserAsync(string username, string password)
    {
        try
        {
            // 1. Obtenir un access token (client credentials)
            var accessToken = await GetClientAccessTokenAsync();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Failed to obtain client access token");
                return null;
            }

            // 2. Préparer la requête de validation
            var validateDto = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(validateDto),
                Encoding.UTF8,
                "application/json");

            // 3. Créer la requête avec le token client
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5002/api/users/validate")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // 4. Envoyer la requête
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UserValidationResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("User {Username} validated successfully", username);
                return result;
            }

            _logger.LogWarning("User validation failed for {Username}. Status: {Status}", username, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Obtient un access token client credentials depuis IdentityServer
    /// Utilise un cache pour éviter de demander un token à chaque appel
    /// </summary>
    private async Task<string?> GetClientAccessTokenAsync()
    {
        // Vérifier si on a un token en cache et qu'il n'est pas expiré
        if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.UtcNow < _tokenExpirationTime)
        {
            return _cachedAccessToken;
        }

        try
        {
            // 1. Découvrir les endpoints d'IdentityServer
            var disco = await _httpClient.GetDiscoveryDocumentAsync("https://localhost:5001");

            if (disco.IsError)
            {
                _logger.LogError("Error discovering IdentityServer: {Error}", disco.Error);
                return null;
            }

            // 2. Demander un token client credentials
            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "identityserver-client",
                ClientSecret = "secret",
                Scope = "taskapi"
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError("Error requesting client token: {Error}", tokenResponse.Error);
                return null;
            }

            // 3. Mettre en cache le token
            _cachedAccessToken = tokenResponse.AccessToken;
            _tokenExpirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 60s de marge

            _logger.LogInformation("Client access token obtained successfully. Expires in {Seconds}s", tokenResponse.ExpiresIn);

            return _cachedAccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining client access token");
            return null;
        }
    }
}

/// <summary>
/// Résultat de la validation utilisateur
/// </summary>
public class UserValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
}