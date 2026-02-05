// NosEmu
// 


using System.ComponentModel.DataAnnotations;

namespace WingsAPI.Communication.Auth
{
    public class BlacklistedHwidDto
    {
        [Required]
        public string HardwareId { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public string Judge { get; set; }
    }
}