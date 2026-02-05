using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using DiscordNotifier.Discord;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Icebreaker;

namespace DiscordNotifier.Consumers.GameEvents
{
    public class LogIcebreakerStartDiscordConsumer : IMessageConsumer<IcebreakerStartMessage>
    {
        private readonly IDiscordWebhookLogsService _discordWebhook;

        public LogIcebreakerStartDiscordConsumer(IDiscordWebhookLogsService discordWebhook) => _discordWebhook = discordWebhook;

        public async Task HandleAsync(IcebreakerStartMessage notification, CancellationToken token)
        {
            if (notification == null)
            {
                EmbedFooterBuilder embedFooterBuilder = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Icebreaker");
                var embedAuthorBuilder = new EmbedAuthorBuilder { IconUrl = StaticHardcodedCode.AvatarUrl };
                var embedBuilders = new List<EmbedBuilder>
                {
                    new()
                    {
                        Author = embedAuthorBuilder,
                        Title = "[ICEBREAKER] An Icebreaker has started!",
                        Description = "Good luck for everyone!",
                        Color = Color.Gold,
                        Footer = embedFooterBuilder,
                        ImageUrl = "https://image.board.gameforge.com/uploads/nostale/de/announcement_nostale_de_e7214229c4ff2dc429db70d758f6e024.jpg",
                        Timestamp = DateTimeOffset.UtcNow
                    }
                };


                await _discordWebhook.PublishLogsEmbedded(LogType.PLAYERS_EVENTS_CHANNEL, embedBuilders);
            }
            else
            {
                EmbedFooterBuilder embedFooterBuilder = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Icebreaker");
                var embedAuthorBuilder = new EmbedAuthorBuilder { IconUrl = StaticHardcodedCode.AvatarUrl };
                var embedBuilders = new List<EmbedBuilder>
                {
                    new()
                    {
                        Author = embedAuthorBuilder,
                        Title = "[ICEBREAKER] An Icebreaker will start in 5 minutes!",
                        Description = "Are you ready for this battle?",
                        Color = Color.Gold,
                        Footer = embedFooterBuilder,
                        ImageUrl = "https://image.board.gameforge.com/uploads/nostale/de/announcement_nostale_de_e7214229c4ff2dc429db70d758f6e024.jpg",
                        Timestamp = DateTimeOffset.UtcNow
                    }
                };


                await _discordWebhook.PublishLogsEmbedded(LogType.PLAYERS_EVENTS_CHANNEL, embedBuilders);
            }
        }
    }
}