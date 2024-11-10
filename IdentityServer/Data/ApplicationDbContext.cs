using IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public virtual DbSet<UserAuthorize> UserAuthorizes { get; set; }
        public virtual DbSet<VoiceData> VoiceDataset { get; set; }
        public virtual DbSet<ImageData> ImageDataset { get; set; }
        public virtual DbSet<SphereData> SphereDataset { get; set; }
        public virtual DbSet<EnvData> EnvDataset { get; set; }
        public virtual DbSet<OdomentryData> OdonomentryDataset { get; set; }
    }
}
