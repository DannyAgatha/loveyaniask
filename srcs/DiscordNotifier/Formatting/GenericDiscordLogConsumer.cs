using System.Threading;
using System.Threading.Tasks;
using DiscordNotifier.Discord;
using PhoenixLib.ServiceBus;
using Plugin.PlayerLogs;

namespace DiscordNotifier.Formatting
{
    public class GenericDiscordLogConsumer<T> : IMessageConsumer<T> where T : IPlayerActionLogMessage
    {
        private readonly IDiscordWebhookLogsService _discordWebhook;
        private readonly IDiscordLogFormatter<T> _formatter;

        public GenericDiscordLogConsumer(IDiscordLogFormatter<T> formatter, IDiscordWebhookLogsService discordWebhook)
        {
            _formatter = formatter;
            _discordWebhook = discordWebhook;
        }

        public async Task HandleAsync(T notification, CancellationToken token)
        {
            if (!_formatter.TryFormat(notification, out string formattedString))
            {
                return;
            }

            await _discordWebhook.PublishLogMessage(_formatter.LogType, formattedString);
        }
    }
}