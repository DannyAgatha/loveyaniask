// NosEmu
// 


using WingsAPI.Packets.Enums;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class ModalPacketsExtensions
    {
        public static string GenerateModalPacket(this IClientSession session, string message, ModalType type) => $"modal {(byte)type} {message}";

        public static void SendModal(this IClientSession session, string message, ModalType type)
        {
            session.SendPacket(session.GenerateModalPacket(message, type));
        }
    }
}