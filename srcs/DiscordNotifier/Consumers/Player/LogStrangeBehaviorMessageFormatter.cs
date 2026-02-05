using System;
using System.Collections.Generic;
using Discord;
using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages;

namespace DiscordNotifier.Consumers.Player;

public class LogStrangeBehaviorEmbedFormatter : IDiscordEmbedLogFormatter<LogStrangeBehaviorMessage>
{
    public LogType LogType => LogType.STRANGE_BEHAVIORS;

    public bool TryFormat(LogStrangeBehaviorMessage message, out List<EmbedBuilder> embeds)
    {
        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle("Strange Behaviors")
            .WithColor(Color.Red)
            .WithThumbnailUrl("https://i.imgur.com/knsZMoy.png")
            .WithImageUrl("https://i.imgur.com/ZZYMR6T.png")
            .WithDescription($"**[{message.SeverityType}]** → {message.Message}")
            .AddField("Channel", message.ChannelId.ToString(), true)
            .AddField("Character", $"{message.CharacterName} ({message.CharacterId})", true)
            .AddField("IP Address", message.IpAddress ?? "Unknown", true)
            .AddField("Created At", message.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), true);

        embeds = [embed];
        return true;
    }
}