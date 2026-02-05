using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.LevelUp;

namespace DiscordNotifier.Consumers.Player
{
    public class LogPlayerLevelUpMessageFormatter : IDiscordLogFormatter<LogLevelUpCharacterMessage>
    {
        public LogType LogType => LogType.FARMING_LEVEL_UP;

        public bool TryFormat(LogLevelUpCharacterMessage message, out string formattedString)
        {
            formattedString =
                $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | {message.CharacterName} {message.LevelType.ToUpperInvariant()} {message.Level}";
            return true;
        }
    }
}