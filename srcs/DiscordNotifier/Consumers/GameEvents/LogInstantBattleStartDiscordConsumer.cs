// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using DiscordNotifier.Discord;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.InstantBattle;

namespace DiscordNotifier.Consumers.GameEvents
{
    public class LogInstantBattleStartDiscordConsumer : IMessageConsumer<InstantBattleStartMessage>
    {
        private readonly IDiscordWebhookLogsService _discordWebhook;

        public LogInstantBattleStartDiscordConsumer(IDiscordWebhookLogsService discordWebhook) => _discordWebhook = discordWebhook;

        public async Task HandleAsync(InstantBattleStartMessage notification, CancellationToken token)
        {
            if (notification.HasNoDelay)
            {
                EmbedFooterBuilder embedFooterBuilder = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Instant Combat");
                var embedAuthorBuilder = new EmbedAuthorBuilder { IconUrl = StaticHardcodedCode.AvatarUrl };
                var embedBuilders = new List<EmbedBuilder>
                {
                    new()
                    {
                        Author = embedAuthorBuilder,
                        Title = "[INSTANT-COMBAT] An instant combat has started!",
                        Description = "May luck be on your side!",
                        Color = Color.DarkBlue,
                        Footer = embedFooterBuilder,
                        ImageUrl = "https://image.board.gameforge.com/uploads/nostale/de/announcement_nostale_de_e7214229c4ff2dc429db70d758f6e024.jpg",
                        Timestamp = DateTimeOffset.UtcNow
                    }
                };


                await _discordWebhook.PublishLogsEmbedded(LogType.PLAYERS_EVENTS_CHANNEL, embedBuilders);
            }
            else
            {
                EmbedFooterBuilder embedFooterBuilder = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Instant Combat");
                var embedAuthorBuilder = new EmbedAuthorBuilder { IconUrl = StaticHardcodedCode.AvatarUrl };
                var embedBuilders = new List<EmbedBuilder>
                {
                    new()
                    {
                        Author = embedAuthorBuilder,
                        Title = "[INSTANT-COMBAT] An instant combat will start in 5 minutes!",
                        Description = "Are you ready for monster waves?",
                        Color = Color.DarkBlue,
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