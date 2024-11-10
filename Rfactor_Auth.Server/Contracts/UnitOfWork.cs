using Rfactor_Auth.Data;
using Rfactor_Auth.Server.Interfaces.Database;
using Rfactor_Auth.Server.Models;

namespace Rfactor_Auth.Server.Contracts
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IRepository<UserAuthorize> UserAuthorizes { get; private set; }
        public IRepository<UserProfile> UserProfiles { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            UserAuthorizes = new Repository<UserAuthorize>(_context);
            UserProfiles = new Repository<UserProfile>(_context);

        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
