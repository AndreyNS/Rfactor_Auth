using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace VoiceAuthentification
{
    public static class Config
    {
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(name: "VoiceApi", displayName: "My API")
            };
        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
                new ApiResource("VoiceApi", "Voice Auth Api")
            };
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client()
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedScopes = { "VoiceApi" }
                }
            };
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static List<TestUser> GetUsers() => 
            new List<TestUser> 
            { 
                new TestUser 
                { 
                    SubjectId = "1", 
                    Username = "testuser", 
                    Password = "password" 
                } 
            };
    }
}
