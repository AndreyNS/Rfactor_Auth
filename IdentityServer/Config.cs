using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace VoiceAuthentification
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
            new ApiScope("api1.read", "Доступ на чтение к API")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "react_client",
                    ClientSecrets = { new Secret("mysecret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = { "http://localhost:3000/callback" },
                    PostLogoutRedirectUris = { "http://localhost:3000/" },
                    AllowedCorsOrigins = { "http://localhost:3000" },
                    AllowedScopes = { "openid", "profile", "api1.read" },
                    AllowAccessTokensViaBrowser = true,
                }
            };

        public static List<TestUser> Users =>
            new List<TestUser>
            {
            new TestUser
            {
                SubjectId = "1",
                Username = "user",
                Password = "password",
            }
            };
    }

}
