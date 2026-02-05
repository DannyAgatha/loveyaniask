using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;

namespace Plugin.Database.Auth.HWID
{
    [Table("blacklisted_hardware_ids", Schema = DatabaseSchemas.CONFIG_AUTH)]
    public class BlacklistedHwidEntity
    {
        [Key]
        [Required]
        public string HardwareId { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public string Judge { get; set; }
    }
}