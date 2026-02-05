using System.Collections.Generic;
using Discord;
using Plugin.PlayerLogs;

namespace DiscordNotifier.Formatting
{
    public interface IDiscordEmbedLogFormatter<TMessage> where TMessage : IPlayerActionLogMessage
    {
        LogType LogType { get; }
        bool TryFormat(TMessage message, out List<EmbedBuilder> embeds);
    }
}