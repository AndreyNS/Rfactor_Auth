using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models
{
    public class UserAuthorize : IdentityUser
    {
        [Key]
        public Guid Guid { get; set; }
        [Required]
        public string UserName {  get; set; }
        public bool IsVoiceAuthentifation { get; set; } = false;
        public bool IsImageAuthentifation {  get; set; } = false;
        public bool IsSphereAuthentifation { get; set; } = false;
        public bool IsEnvironmentAuthentifation { get; set; } = false;
        public bool IsOdomentryAuthentifation { get; set; } = false;

        public virtual VoiceData? Voice { get; set; }
        public virtual ImageData? Image { get; set; }
        public virtual SphereData? Sphere { get; set; }
        public virtual EnvData? Environment { get; set; }
        public virtual OdomentryData? Odomentry { get; set; }
    }
}
