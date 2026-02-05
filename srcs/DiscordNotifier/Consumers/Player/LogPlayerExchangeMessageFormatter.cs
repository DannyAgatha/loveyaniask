using System;
using System.Collections.Generic;
using System.Linq;
using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Player;

namespace DiscordNotifier.Consumers.Player
{
    public class LogPlayerExchangeMessageFormatter : IDiscordLogFormatter<LogPlayerExchangeMessage>
    {
        public LogType LogType => LogType.ITEM_EXCHANGE;

        public bool TryFormat(LogPlayerExchangeMessage message, out string formattedString)
        {
            var itemsFormatted = message.Items.Select(item => $"VNUM: {item.ItemInstance.ItemVNum}, Quantity: {item.Amount}").ToList();
            string itemsGivenString = itemsFormatted.Any() ? $"\n{string.Join("\n", itemsFormatted)}" : "None";
            
            var targetItemsFormatted = message.TargetItems.Select(item => $"VNUM: {item.ItemInstance.ItemVNum}, Quantity: {item.Amount}").ToList();
            string itemsReceivedString = targetItemsFormatted.Any() ? $"\n{string.Join("\n", targetItemsFormatted)}" : "None";

            formattedString = $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId}\n\n" +
                $"Initiator: {message.CharacterName} (ID: {message.CharacterId})\n" +
                $"Items Given: {itemsGivenString}\n" +
                $"Gold: {message.Gold}\n" +
                $"Bank Gold: {message.BankGold}\n\n" +
                $"Target: {message.TargetCharacterName} (ID: {message.TargetCharacterId})\n" +
                $"Items Received: {itemsReceivedString}\n" +
                $"Gold: {message.TargetGold}\n" +
                $"Bank Gold: {message.TargetBankGold}";

            return true;
        }
    }
}