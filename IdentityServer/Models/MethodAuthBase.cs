using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServer.Models
{
    public class MethodAuthBase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserAuthorize")]
        public Guid UserGuid { get; set; }
        public virtual UserAuthorize UserAuthorize { get; set; }
    }
}
