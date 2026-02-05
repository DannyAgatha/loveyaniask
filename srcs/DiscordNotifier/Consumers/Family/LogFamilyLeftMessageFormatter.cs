using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyLeftMessageFormatter : IDiscordLogFormatter<LogFamilyLeftMessage>
    {
        public LogType LogType => LogType.FAMILY_LEFT;

        public bool TryFormat(LogFamilyLeftMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | "
                + $"PLAYER: {message.CharacterName} | FAM_ID: {message.FamilyId}";
            return true;
        }
    }
}