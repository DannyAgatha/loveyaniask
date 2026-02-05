using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.CharacterExtensions
{
    public static class FriendsExtensions
    {
        public static string GenerateFriendOnlineInfo(this IClientSession session, long characterId, string characterName, bool isOnline)
            => $"finfo {characterId}.{(isOnline ? 1 : 0)}.{characterName}";

        public static void SendFriendOnlineInfo(this IClientSession session, long characterId, string characterName, bool isOnline) =>
            session.SendPacket(session.GenerateFriendOnlineInfo(characterId, characterName, isOnline));
    }
}