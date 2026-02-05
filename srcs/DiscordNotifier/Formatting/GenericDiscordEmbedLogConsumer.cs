using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using DiscordNotifier.Discord;
using PhoenixLib.ServiceBus;
using Plugin.PlayerLogs;

namespace DiscordNotifier.Formatting
{
    public class GenericDiscordEmbedLogConsumer<T> : IMessageConsumer<T> where T : IPlayerActionLogMessage
    {
        private readonly IDiscordWebhookLogsService _discordWebhook;
        private readonly IDiscordEmbedLogFormatter<T> _formatter;

        public GenericDiscordEmbedLogConsumer(IDiscordWebhookLogsService discordWebhook, IDiscordEmbedLogFormatter<T> formatter)
        {
            _discordWebhook = discordWebhook;
            _formatter = formatter;
        }

        public async Task HandleAsync(T notification, CancellationToken token)
        {
            if (!_formatter.TryFormat(notification, out List<EmbedBuilder> embeds))
            {
                return;
            }

            await _discordWebhook.PublishLogsEmbedded(_formatter.LogType, embeds);
        }
    }
}