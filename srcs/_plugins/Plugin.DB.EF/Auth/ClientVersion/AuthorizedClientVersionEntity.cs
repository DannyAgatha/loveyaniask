// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;

namespace Plugin.Database.Auth.ClientVersion
{
    [Table("authorized_client_versions", Schema = DatabaseSchemas.CONFIG_AUTH)]
    public class AuthorizedClientVersionEntity : ILongEntity
    {
        [Required]
        public string ClientVersion { get; set; }

        [Required]
        public string ExecutableHash { get; set; }

        [Required]
        public string DllHash { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}