using IdentityServer.Data;
using IdentityServer.Interfaces.Database;
using IdentityServer.Models;

namespace IdentityServer.Contracts
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IRepository<UserAuthorize> UserAuthorizes { get; private set; }
        public IRepository<VoiceData> VoiceDataset { get; private set; }
        public IRepository<ImageData> ImageDataset { get; private set; }
        public IRepository<SphereData> SphereDataset { get; private set; }
        public IRepository<EnvData> EnvDataset { get; private set; }
        public IRepository<OdomentryData> OdonomentryDataset { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            UserAuthorizes = new Repository<UserAuthorize>(_context);
            VoiceDataset = new Repository<VoiceData>(_context);
            ImageDataset = new Repository<ImageData>(_context);
            SphereDataset = new Repository<SphereData>(_context);
            EnvDataset = new Repository<EnvData>(_context);
            OdonomentryDataset = new Repository<OdomentryData>(_context);

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
