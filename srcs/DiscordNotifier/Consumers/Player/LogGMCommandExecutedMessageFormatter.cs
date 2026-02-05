using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages;

namespace DiscordNotifier.Consumers.Player
{
    public class LogGmCommandExecutedMessageFormatter : IDiscordLogFormatter<LogGmCommandExecutedMessage>
    {
        public LogType LogType => LogType.COMMANDS_GM_COMMAND_EXECUTED;

        public bool TryFormat(LogGmCommandExecutedMessage message, out string formattedString)
        {
            formattedString =
                $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | P.Authority: {message.PlayerAuthority} | C.Authority: [{message.CommandAuthority}] {message.CharacterName} | {message.Command}";
            return true;
        }
    }
}