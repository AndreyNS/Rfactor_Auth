namespace Rfactor_Auth.Server.Interfaces
{
    public interface ICache
    {
        void AddTokenToBlacklist(string token, int lifeTime);
        bool IsTokenBlacklisted(string token);
    }
}
