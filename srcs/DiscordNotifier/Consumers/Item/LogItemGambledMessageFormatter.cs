using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Upgrade;

namespace DiscordNotifier.Consumers.Item
{
    public class LogItemGambledMessageFormatter : IDiscordLogFormatter<LogItemGambledMessage>
    {
        public LogType LogType => LogType.ITEM_GAMBLED;

        public bool TryFormat(LogItemGambledMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | PLAYER: {message.CharacterName} | ITEM: {message.ItemVnum} | MODE: {message.Mode} | "
                + $"PROTECTION: {message.Protection}{(message.Amulet != null ? " | AMULET: " + message.Amulet : "")} | SUCCEED: {message.Succeed} | RARITY: {message.OriginalRarity}";
            return true;
        }
    }
}