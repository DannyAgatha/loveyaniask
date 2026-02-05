// NosEmu
// 


using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class InfoPacketsExtensions
    {
        public static string GenerateSayPacket(this IClientSession session, string msg, ChatMessageColorType color) =>
            $"say {(byte)session.PlayerEntity.Type} {session.PlayerEntity.Id} {(byte)color} {msg}";

        public static string GenerateInfoPacket(this IClientSession session, string message) => $"info {message}";
    }
}