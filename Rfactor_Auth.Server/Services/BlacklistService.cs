using Microsoft.Extensions.Caching.Memory;
using Rfactor_Auth.Server.Interfaces;

namespace Rfactor_Auth.Server.Services
{
    public class BlacklistService : ICache
    {
        private readonly IMemoryCache _cache;

        public BlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddTokenToBlacklist(string token, int lifeTime)
        {
            _cache.Set(token, true, TimeSpan.FromMinutes(lifeTime));
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _cache.TryGetValue(token, out _);
        }
    }
}
