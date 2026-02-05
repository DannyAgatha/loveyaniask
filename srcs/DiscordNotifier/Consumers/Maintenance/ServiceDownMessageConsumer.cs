// NosEmu
// 


using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Webhook;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;

namespace DiscordNotifier.Consumers.Maintenance
{
    public class ServiceDownMessageConsumer : IMessageConsumer<ServiceDownMessage>
    {
        private static readonly string _webhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_HEALTHCHECK_URL");

        public async Task HandleAsync(ServiceDownMessage notification, CancellationToken token)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                return;
            }

            var client = new DiscordWebhookClient(_webhookUrl);
            await client.SendMessageAsync($"```\n[{notification.LastUpdate:yyyy-MM-dd HH:mm:ss}][HEALTHCHECK] {notification.ServiceName} IS OFFLINE!```");
        }
    }
}