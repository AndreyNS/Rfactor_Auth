namespace Rfactor_Auth.Server.Interfaces
{
    public interface ICrypto
    {
        public byte[] GenerateSalt();
        string GenerateRefreshToken();
        string GenerateAccessToken(string username);
        string HashPassword(string password, byte[] salt);
    }
}
