using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyDisbandedMessageFormatter : IDiscordLogFormatter<LogFamilyDisbandedMessage>
    {
        public LogType LogType => LogType.FAMILY_DISBANDED;

        public bool TryFormat(LogFamilyDisbandedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | "
                + $"PLAYER: {message.CharacterName} | FAM_ID: {message.FamilyId}";
            return true;
        }
    }
}