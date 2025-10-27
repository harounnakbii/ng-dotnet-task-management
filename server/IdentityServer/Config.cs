using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    // ===========================
    // IDENTITY RESOURCES
    // ===========================
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    // ===========================
    // API SCOPES
    // ===========================
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("taskapi", "Task Management API")
        };

    // ===========================
    // API RESOURCES
    // ===========================
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("taskapi", "Task Management API")
            {
                Scopes = { "taskapi" },
                UserClaims = { "name", "email", "role" }
            }
        };

    // ===========================
    // CLIENTS
    // ===========================
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Angular SPA Client
            new Client
            {
                ClientId = "angular-client",
                ClientName = "Angular Task Management",

                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireClientSecret = false, // SPA = public client
                RequirePkce = true, // PKCE obligatoire pour SPA
                
                // URLs Angular
                RedirectUris = { "http://localhost:4200" },
                PostLogoutRedirectUris = { "http://localhost:4200" },
                AllowedCorsOrigins = { "http://localhost:4200" },
                
                // Scopes autorisés
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "taskapi"
                },

                AllowOfflineAccess = true, // Refresh tokens
                AccessTokenLifetime = 3600, // 1 heure
                AllowAccessTokensViaBrowser = true
            },
            
            // Client interne IdentityServer (pour appeler Task API)
            new Client
            {
                ClientId = "identityserver-client",
                ClientName = "IdentityServer Internal Client",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "taskapi" }
            }
        };
}