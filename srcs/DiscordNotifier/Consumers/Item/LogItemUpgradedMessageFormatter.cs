using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Upgrade;

namespace DiscordNotifier.Consumers.Item
{
    public class LogItemUpgradedMessageFormatter : IDiscordLogFormatter<LogItemUpgradedMessage>
    {
        public LogType LogType => LogType.ITEM_UPGRADED;

        public bool TryFormat(LogItemUpgradedMessage message, out string formattedString)
        {
            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | PLAYER: {message.CharacterName} | ITEM: {message.Item.ItemVNum} | MODE: {message.Mode} | "
                + $"PROTECTION: {message.Protection} | HAS_AMULET: {message.HasAmulet} | ORIGINAL_UPGRADE: {message.OriginalUpgrade} | RESULT: {message.Result}";
            return true;
        }
    }
}