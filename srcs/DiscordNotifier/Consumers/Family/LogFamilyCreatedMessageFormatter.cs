using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyCreatedMessageFormatter : IDiscordLogFormatter<LogFamilyCreatedMessage>
    {
        public LogType LogType => LogType.FAMILY_CREATED;

        public bool TryFormat(LogFamilyCreatedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | HEAD: {message.CharacterName} | DEPUTIES: {string.Join(", ", message.DeputiesIds)} | "
                + $"FAM_ID: {message.FamilyId} | FAM_NAME: {message.FamilyName}";
            return true;
        }
    }
}