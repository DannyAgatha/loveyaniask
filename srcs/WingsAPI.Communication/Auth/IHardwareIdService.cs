using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Auth
{
    public interface IHardwareIdService
    {
        Task<bool> SynchronizeWithDbAsync(IEnumerable<BlacklistedHwidDto> dtos);
        Task<bool> CanLogin(string hardwareId);
        Task RegisterHardwareId(string hardwareId);
        Task UnregisterHardwareId(string hardwareId);
    }
}