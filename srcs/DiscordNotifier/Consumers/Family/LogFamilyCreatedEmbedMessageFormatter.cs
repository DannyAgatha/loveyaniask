using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Family;

namespace DiscordNotifier.Consumers.Family
{
    public class LogFamilyCreatedEmbedMessageFormatter : IDiscordEmbedLogFormatter<LogFamilyCreatedMessage>
    {
        public LogType LogType => LogType.PLAYERS_EVENTS_CHANNEL;
        
        public bool TryFormat(LogFamilyCreatedMessage message, out List<EmbedBuilder> embeds)
        {
            embeds = new List<EmbedBuilder>
            {
                new()
                {
                    Author = new EmbedAuthorBuilder { IconUrl = Environment.GetEnvironmentVariable("FAMILY_CREATE_AUTHOR_ICON") },
                    Title = $":small_blue_diamond: [{message.FamilyName}] Family Arrived! :small_blue_diamond:",
                    Description = $"Great news, [{message.FamilyName}] has emerged in the world of Evelyum! :partying_face: :tada:",
                    Color = new Color(1, 42, 254),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Environment.GetEnvironmentVariable("FAMILY_CREATE_FOOTER_ICON")).WithText("Family Service"),
                    Fields = new List<EmbedFieldBuilder> 
                    {
                        string.IsNullOrEmpty(message.CharacterName) ? null : new EmbedFieldBuilder
                        {
                            Name = ":mag_right: Family Head",
                            Value = $":crown: {message.CharacterName} Lv.{message.Level}(+{message.HeroLevel}) - Class: {message.Class} :bust_in_silhouette:",
                            IsInline = false
                        }
                    }.Where(x => x != null).ToList(),
                    ImageUrl = Environment.GetEnvironmentVariable("FAMILY_CREATE_EMBED_IMAGE"),
                    ThumbnailUrl = Environment.GetEnvironmentVariable("FAMILY_CREATE_THUMBNAIL"),
                    Timestamp = DateTimeOffset.Now
                }
            };
            return true;
        }

    }
}