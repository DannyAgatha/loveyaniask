using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using DiscordNotifier.Discord;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;

namespace DiscordNotifier.Consumers.Maintenance
{
    public class ServiceMaintenanceNotificationMessageConsumer : IMessageConsumer<ServiceMaintenanceNotificationMessage>
    {
        private const string ThumbnailUrl = "https://avatars.githubusercontent.com/u/115504336?s=96&v=4";

        private static readonly EmbedAuthorBuilder Author = new() { IconUrl = StaticHardcodedCode.AvatarUrl };
        private static readonly EmbedFooterBuilder DefaultFooter = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Timestamp:");
        private static readonly EmbedFooterBuilder FooterForSchedule = new EmbedFooterBuilder().WithIconUrl(StaticHardcodedCode.AvatarUrl).WithText("Maintenance scheduled for:");
        private readonly IDiscordWebhookLogsService _discordWebhook;

        public ServiceMaintenanceNotificationMessageConsumer(IDiscordWebhookLogsService discordWebhook) => _discordWebhook = discordWebhook;

        public async Task HandleAsync(ServiceMaintenanceNotificationMessage notification, CancellationToken token)
        {
            DateTime scheduledMaintenanceDateTime = DateTime.UtcNow + notification.TimeLeft;

            EmbedBuilder embedBuilder = notification.NotificationType switch
            {
                ServiceMaintenanceNotificationType.Rescheduled => new EmbedBuilder
                {
                    Author = Author,
                    Title = "[Ascension Maintenance] The maintenance has been re-scheduled!",
                    Description = GenerateScheduleDescription(scheduledMaintenanceDateTime, notification.Reason),
                    Color = Color.Gold,
                    ThumbnailUrl = ThumbnailUrl,
                    Footer = FooterForSchedule,
                    Timestamp = new DateTimeOffset(scheduledMaintenanceDateTime, TimeSpan.Zero)
                },
                ServiceMaintenanceNotificationType.ScheduleStopped => new EmbedBuilder
                {
                    Author = Author,
                    Title = "[Ascension Maintenance] Maintenance canceled!",
                    Description = string.IsNullOrEmpty(notification.Reason) ? string.Empty : $"Reason: {notification.Reason}",
                    Color = Color.Red,
                    ThumbnailUrl = ThumbnailUrl,
                    Footer = DefaultFooter,
                    Timestamp = new DateTimeOffset(scheduledMaintenanceDateTime, TimeSpan.Zero)
                },
                ServiceMaintenanceNotificationType.Executed => new EmbedBuilder
                {
                    Author = Author,
                    Title = "[Ascension Maintenance] The Server is now in Maintenance mode!",
                    Color = Color.DarkBlue,
                    ThumbnailUrl = ThumbnailUrl,
                    Footer = DefaultFooter,
                    Timestamp = new DateTimeOffset(scheduledMaintenanceDateTime, TimeSpan.Zero)
                },
                ServiceMaintenanceNotificationType.EmergencyExecuted => new EmbedBuilder
                {
                    Author = Author,
                    Title = "[Ascension Maintenance] An emergency maintenance has been called!",
                    Description = $"An unexpected maintenance has been executed, sorry for any inconvenience it may have cause.\n**Reason:** {notification.Reason}",
                    Color = Color.Purple,
                    ThumbnailUrl = ThumbnailUrl,
                    Footer = DefaultFooter,
                    Timestamp = new DateTimeOffset(scheduledMaintenanceDateTime, TimeSpan.Zero)
                },
                ServiceMaintenanceNotificationType.Lifted => new EmbedBuilder
                {
                    Author = Author,
                    Title = "[Ascension Maintenance] The Server's maintenance has been lifted!",
                    Color = Color.Green,
                    ThumbnailUrl = ThumbnailUrl,
                    Footer = DefaultFooter,
                    Timestamp = new DateTimeOffset(scheduledMaintenanceDateTime, TimeSpan.Zero)
                },
                _ => null
            };

            if (embedBuilder == null)
            {
                return;
            }

            await _discordWebhook.PublishLogEmbedded(LogType.PLAYERS_EVENTS_CHANNEL, embedBuilder);
        }

        private static string GenerateScheduleDescription(DateTime scheduledMaintenanceDateTime, string reason) => $"**Date and time:** {scheduledMaintenanceDateTime:U} (UTC)\n**Reason:** {reason}";
    }
}