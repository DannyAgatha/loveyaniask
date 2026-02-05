using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyKickedMessageFormatter : IDiscordLogFormatter<LogFamilyKickedMessage>
    {
        public LogType LogType => LogType.FAMILY_KICK;

        public bool TryFormat(LogFamilyKickedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | "
                + $"PLAYER: {message.CharacterName} | FAM_ID: {message.FamilyId} | KICKED_NAME: {message.KickedMemberName} | KICKED_ID: {message.KickedMemberId}";
            return true;
        }
    }
}