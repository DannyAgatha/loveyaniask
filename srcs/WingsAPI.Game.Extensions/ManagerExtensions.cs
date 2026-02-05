using WingsAPI.Communication;
using WingsEmu.Game._enum;

namespace WingsAPI.Game.Extensions
{
    public static class ManagerExtensions
    {
        public static ManagerResponseType ToManagerType(this RpcResponseType responseType)
        {
            return responseType switch
            {
                RpcResponseType.MAINTENANCE_MODE => ManagerResponseType.Maintenance,
                RpcResponseType.SUCCESS => ManagerResponseType.Success,
                _ => ManagerResponseType.Error
            };
        }
    }
}