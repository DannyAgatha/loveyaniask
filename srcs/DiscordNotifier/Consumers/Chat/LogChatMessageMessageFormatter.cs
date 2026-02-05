// NosEmu
// 


using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game._playerActionLogs;

namespace DiscordNotifier.Consumers.Chat
{
    public class LogChatMessageMessageFormatter : IDiscordLogFormatter<LogPlayerChatMessage>
    {
        public LogType LogType { get; private set; }

        public bool TryFormat(LogPlayerChatMessage message, out string formattedString)
        {
            LogType = message.ChatType switch
            {
                ChatType.General => LogType.CHAT_GENERAL,
                ChatType.HeroChat => LogType.CHAT_GENERAL,
                ChatType.SpeechBubble => LogType.CHAT_GENERAL,

                ChatType.Whisper => LogType.CHAT_WHISPERS,
                ChatType.FriendChat => LogType.CHAT_FRIENDS,

                ChatType.Shout => LogType.CHAT_SPEAKERS,

                ChatType.FamilyChat => LogType.CHAT_FAMILIES,
                ChatType.GroupChat => LogType.CHAT_GROUPS
            };

            formattedString = $"[ChannelId: '{message.ChannelId.ToString()}']" +
                $"[ChatType: '{message.ChatType.ToString()}']" +
                $"[CharacterId: '{message.CharacterId.ToString()}'{(message.TargetCharacterId.HasValue ? $" -> TargetId: '{message.TargetCharacterId.Value.ToString()}'" : string.Empty)}]" +
                $" {message.Message}";

            return true;
        }
    }
}