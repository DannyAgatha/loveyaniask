using System.Collections.Generic;
using Discord;
using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Act4;

namespace DiscordNotifier.Consumers.Act4
{
    public class LogAct4PvpKillConsumer : IDiscordEmbedLogFormatter<LogAct4PvpKillMessage>
    {
        public LogType LogType => LogType.ACT4_PVP_KILL;

        public bool TryFormat(LogAct4PvpKillMessage message, out List<EmbedBuilder> embeds)
        {
            // Interpretando la facción del asesino
            string killerFactionText = message.KillerFaction == "1" ? "Angel" : message.KillerFaction == "2" ? "Demon" : "Desconocido";

            embeds = new List<EmbedBuilder>
            {
                new EmbedBuilder
                {
                    Title = $"PvP Kill en Acto 4",
                    Description = $"**{message.CharacterName}** de la facción **{killerFactionText}** ha logrado una baja en PvP.",
                    Color = killerFactionText == "Angel" ? Color.Blue : killerFactionText == "Demon" ? Color.Red : Color.LightGrey, // Color basado en la facción
                    Footer = new EmbedFooterBuilder().WithText($"ID del canal: {message.ChannelId}"),
                    Timestamp = message.CreatedAt
                }
            };

            return true; // Indica que la operación fue exitosa
        }
    }
}