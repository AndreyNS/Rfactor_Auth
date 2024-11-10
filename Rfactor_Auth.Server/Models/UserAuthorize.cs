using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Rfactor_Auth.Server.Models
{
    public class UserAuthorize : IdentityUser
    {
        [Key]
        public Guid Guid { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }     
        [Required]
        public string Salt { get; set; }
        public virtual UserProfile? UserProfile { get; set; }
    }
}
