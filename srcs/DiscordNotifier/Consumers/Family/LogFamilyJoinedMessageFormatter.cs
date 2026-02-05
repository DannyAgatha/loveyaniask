using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyJoinedMessageFormatter : IDiscordLogFormatter<LogFamilyJoinedMessage>
    {
        public LogType LogType => LogType.FAMILY_JOINED;

        public bool TryFormat(LogFamilyJoinedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | "
                + $"PLAYER: {message.CharacterName} | INVITER_ID: {message.InviterId} | FAM_ID: {message.FamilyId}";
            return true;
        }
    }
}