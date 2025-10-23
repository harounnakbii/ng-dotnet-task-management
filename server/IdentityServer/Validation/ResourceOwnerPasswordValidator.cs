using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityServer.Services;
using System.Security.Claims;

namespace IdentityServer.Validation;

public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly UserValidationService _userValidationService;
    private readonly ILogger<ResourceOwnerPasswordValidator> _logger;

    public ResourceOwnerPasswordValidator(
        UserValidationService userValidationService,
        ILogger<ResourceOwnerPasswordValidator> logger)
    {
        _userValidationService = userValidationService;
        _logger = logger;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        try
        {
            var result = await _userValidationService.ValidateUserAsync(
                context.UserName,
                context.Password);

            if (result?.IsValid == true && result.UserId.HasValue)
            {
                context.Result = new GrantValidationResult(
                    subject: result.UserId.Value.ToString(),
                    authenticationMethod: "custom",
                    claims: new[]
                    {
                        new Claim("sub", result.UserId.Value.ToString()),
                        new Claim("name", result.Name ?? ""),
                        new Claim("email", result.Email ?? "")
                    });

                _logger.LogInformation("User {Email} validated successfully", result.Email);
            }
            else
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    result?.ErrorMessage ?? "Invalid username or password");

                _logger.LogWarning("Failed validation for user {Username}", context.UserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user {Username}", context.UserName);
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Authentication error");
        }
    }
}