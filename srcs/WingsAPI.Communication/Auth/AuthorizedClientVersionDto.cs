using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;

namespace WingsAPI.Communication.Auth
{
    public class AuthorizedClientVersionDto : ILongDto
    {
        public string ClientVersion { get; set; }

        public string ExecutableHash { get; set; }

        public string DllHash { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}