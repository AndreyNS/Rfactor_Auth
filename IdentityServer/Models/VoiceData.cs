using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServer.Models
{
    public class VoiceData : MethodAuthBase
    {
        [Required]
        public string Voice { get; set; }
    }
}
