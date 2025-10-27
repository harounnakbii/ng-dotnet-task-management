using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace IdentityServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserValidationService _userValidationService;

    public ProfileService(UserValidationService userValidationService)
    {
        _userValidationService = userValidationService;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(sub))
        {
            // Add claims to token
            context.IssuedClaims.AddRange(context.Subject.Claims);
        }

        await Task.CompletedTask;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        await Task.CompletedTask;
    }
}