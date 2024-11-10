using Rfactor_Auth.Server.Models;

namespace Rfactor_Auth.Server.Interfaces.Database
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<UserAuthorize> UserAuthorizes { get; }
        IRepository<UserProfile> UserProfiles { get; }
        Task<int> SaveChangesAsync();
    }
}
