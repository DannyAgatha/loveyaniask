using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyMessageMessageFormatter : IDiscordLogFormatter<LogFamilyMessageMessage>
    {
        public LogType LogType => LogType.FAMILY_MESSAGES;

        public bool TryFormat(LogFamilyMessageMessage message, out string formattedString)
        {
            formattedString = $"[{message.CreatedAt:yyyy-MM-dd HH:mm:ss}] [CHANNEL {message.ChannelId}] [FAM_ID: {message.FamilyId}]\n"
                + $"**Player**: {message.CharacterName}\n"
                + $"**MessageType**: {message.FamilyMessageType}\n"
                + $"**Message**: {message.Message}\n";
            return true;
        }
    }
}