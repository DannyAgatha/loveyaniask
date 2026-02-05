// NosEmu
// 


using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class DelayPacketsExtensions
    {
        public static string GenerateDelayPacket(this IClientSession session, int delay, int type, string argument) => $"delay {delay} {type} {argument}";
    }
}