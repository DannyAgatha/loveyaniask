using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game.Chat;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public sealed class LogPlayerChatMessageEnricher : ILogMessageEnricher<ChatGenericEvent, LogPlayerChatMessage>
    {
        public void Enrich(LogPlayerChatMessage message, ChatGenericEvent e)
        {
            message.Message = e.Message;
            message.ChatType = e.ChatType;
            message.TargetCharacterId = e.TargetCharacterId;
        }
    }
}