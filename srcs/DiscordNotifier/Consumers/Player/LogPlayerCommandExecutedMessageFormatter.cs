using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Player;

namespace DiscordNotifier.Consumers.Player
{
    public class LogPlayerCommandExecutedMessageFormatter : IDiscordLogFormatter<LogPlayerCommandExecutedMessage>
    {
        public LogType LogType => LogType.COMMANDS_PLAYER_COMMAND_EXECUTED;

        public bool TryFormat(LogPlayerCommandExecutedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | {message.CharacterName} | {message.Command}";
            return true;
        }
    }
}