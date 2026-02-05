// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using PhoenixLib.Logging;

namespace DiscordNotifier.Discord
{
    public class DiscordWebhookLogsService : IDiscordWebhookLogsService
    {
        private readonly DiscordWebhookConfiguration _configs;

        public DiscordWebhookLogsService(DiscordWebhookConfiguration configs) => _configs = configs;

        public async Task PublishLogMessage(LogType logType, string message)
        {
            try
            {
                if (!_configs.TryGetValue(logType, out string webhookUrl) || string.IsNullOrEmpty(webhookUrl))
                {
                    return;
                }

                using var webhookClient = new DiscordWebhookClient(webhookUrl);
                await webhookClient.SendMessageAsync($"```\n{message}\n```");
            }
            catch (Exception e)
            {
                Log.Error($"[LogsServer] PublishLogEmbedded {logType}", e);
            }
        }

        public async Task PublishLogEmbedded(LogType logType, EmbedBuilder embed)
        {
            try
            {
                if (!_configs.TryGetValue(logType, out string webhookUrl) || string.IsNullOrEmpty(webhookUrl))
                {
                    return;
                }

                using var webhookClient = new DiscordWebhookClient(webhookUrl);
                await webhookClient.SendMessageAsync(embeds: new[] { embed.Build() });
            }
            catch (Exception e)
            {
                Log.Error($"[LogsServer] PublishLogEmbedded {logType}", e);
            }
        }

        public async Task PublishLogsEmbedded(LogType logType, List<EmbedBuilder> embeds)
        {
            var builtEmbeds = new List<Embed>();
            foreach (EmbedBuilder embed in embeds)
            {
                builtEmbeds.Add(embed.Build());
            }

            try
            {
                if (!_configs.TryGetValue(logType, out string webhookUrl) || string.IsNullOrEmpty(webhookUrl))
                {
                    return;
                }

                using var webhookClient = new DiscordWebhookClient(webhookUrl);
                await webhookClient.SendMessageAsync(embeds: builtEmbeds);
            }
            catch (Exception e)
            {
                Log.Error($"[LogsServer] PublishLogsEmbedded {logType}", e);
            }
        }
    }
}