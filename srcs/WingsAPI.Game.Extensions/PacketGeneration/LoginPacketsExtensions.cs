// NosEmu
// 


using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class LoginPacketsExtensions
    {
        public static string GenerateFailcPacket(this IClientSession session, LoginFailType failType) => $"failc {(short)failType}";
    }
}