using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace DiscordNotifier.Discord
{
    public interface IDiscordWebhookLogsService
    {
        Task PublishLogMessage(LogType logType, string message);
        Task PublishLogEmbedded(LogType logType, EmbedBuilder embed);
        Task PublishLogsEmbedded(LogType logType, List<EmbedBuilder> embeds);
    }
}