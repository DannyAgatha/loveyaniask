// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums.Relations;

namespace WingsAPI.Game.Extensions.RelationsExtensions
{
    public static class RelationsExtensions
    {
        public static async Task AddRelationAsync(this IClientSession session, long targetCharacterId, CharacterRelationType type)
        {
            await session.EmitEventAsync(new AddRelationEvent(targetCharacterId, type));
        }

        public static async Task RemoveRelationAsync(this IClientSession session, long targetCharacterId, CharacterRelationType type)
        {
            await session.EmitEventAsync(new RemoveRelationEvent(targetCharacterId, type));
        }

        public static string GenerateFriendMessage(this IClientSession session, long targetId, string message) => $"talk {targetId} {message}";

        public static void SendFriendMessage(this IClientSession session, long targetId, string message) => session.SendPacket(session.GenerateFriendMessage(targetId, message));
    }
}