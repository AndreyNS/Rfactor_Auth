using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rfactor_Auth.Server.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        public string FIO { get; set; }
        public string Email { get; set; }

        [ForeignKey("UserAuthorize")]
        public Guid? UserGuid { get; set; }
        public virtual UserAuthorize? UserAuthorize { get; set; }
    }
}
