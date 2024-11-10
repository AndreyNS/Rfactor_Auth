
using IdentityServer.Models;

namespace IdentityServer.Interfaces.Database
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<UserAuthorize> UserAuthorizes { get; }
        IRepository<VoiceData> VoiceDataset { get; }
        IRepository<ImageData> ImageDataset { get; }
        IRepository<SphereData> SphereDataset { get; }
        IRepository<EnvData> EnvDataset { get; }
        IRepository<OdomentryData> OdonomentryDataset { get; }
        Task<int> SaveChangesAsync();
    }
}
