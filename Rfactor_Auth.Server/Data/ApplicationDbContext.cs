using Rfactor_Auth.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Rfactor_Auth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public virtual DbSet<UserAuthorize> UserAuthorizes { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
    }
}
