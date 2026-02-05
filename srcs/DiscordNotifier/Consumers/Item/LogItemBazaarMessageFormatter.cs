using System;
using System.Collections.Generic;
using Discord;
using System.Globalization;
using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace DiscordNotifier.Consumers.Item
{
    public class LogItemBazaarMessageFormatter : IDiscordEmbedLogFormatter<LogBazaarItemInsertedMessage>
    {
        public LogType LogType => LogType.ITEM_BAZAAR;
        
        private readonly List<(int ItemVNum, int HoldingVNum)> itemToHoldingMapping =
        [
            (4240, 901), // Warrior Specialist Card
            (4240, 902), // Ninja Specialist Card
            (4240, 909), // Crusader Specialist Card
            (4240, 910), // Berserker Specialist Card
            (4240, 4500), // Gladiator Specialist Card
            (4240, 4497), // Battle Monk Specialist Card
            (4240, 4493), // Death Reaper Specialist Card
            (4240, 4489), // Renegade Specialist Card
            (4240, 4581), // Waterfall Berserker Specialist Card
            (4240, 8521) // Dragon Knight Specialist Card
        ];

        public bool TryFormat(LogBazaarItemInsertedMessage message, out List<EmbedBuilder> embeds)
        {
            int originalItemVNum = message.ItemInstance.ItemVNum;
            int thumbnailVNum = originalItemVNum;
            
            bool isSpecialItem = itemToHoldingMapping.Exists(item => item.ItemVNum == originalItemVNum && item.HoldingVNum == message.ItemInstance.HoldingVNum);
            
            foreach ((int ItemVNum, int HoldingVNum) in itemToHoldingMapping)
            {
                if (originalItemVNum != ItemVNum || message.ItemInstance.HoldingVNum != HoldingVNum)
                {
                    continue;
                }

                thumbnailVNum = HoldingVNum;
                break;
            }
            
            string rarityName = message.ItemInstance.Rarity switch
            {
                0 => "Basic",
                1 => "Useful",
                2 => "Good",
                3 => "High Quality",
                4 => "Excellent",
                5 => "Ancient",
                6 => "Mysterious",
                7 => "Legendary",
                8 => "Phenomenal",
                _ => "Unknown"
            };


            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("📦 New Bazaar Item Inserted")
                .WithColor(new Color(0x3498db))
                .WithThumbnailUrl($"https://itempicker.atlagaming.eu/api/items/icon/{thumbnailVNum}")
                .AddField("Channel", $"📺 {message.ChannelId}", true)
                .AddField("Seller", $"👤 {message.CharacterName}", true)
                .AddField("Price", $"💰 {FormatWithKSuffix(message.Price)}", true)
                .AddField("Quantity", $" {message.Quantity}", true)
                .AddField("Taxes", $" {FormatWithKSuffix(message.Taxes)}", true)
                .AddField("Upgrade", $" {message.ItemInstance.Upgrade}", true);
                embedBuilder.AddField("Rarity", rarityName, true);
            
            if (isSpecialItem)
            {
                embedBuilder
                    .AddField("SpDamage", $" {message.ItemInstance.SpDamage}", true)
                    .AddField("SpDefence", $" {message.ItemInstance.SpDefence}", true)
                    .AddField("SpElement", $" {message.ItemInstance.SpElement}", true)
                    .AddField("SpHP", $" {message.ItemInstance.SpHP}", true)
                    .AddField("SpFire", $" {message.ItemInstance.SpFire}", true)
                    .AddField("SpWater", $" {message.ItemInstance.SpWater}", true)
                    .AddField("SpLight", $" {message.ItemInstance.SpLight}", true)
                    .AddField("SpDark", $" {message.ItemInstance.SpDark}", true)
                    .AddField("SpStoneUpgrade", $" {message.ItemInstance.SpStoneUpgrade}", true);
            }
            
            embedBuilder.WithImageUrl("https://cdn.cloudflare.steamstatic.com/steam/apps/550470/header.jpg?t=1702309544")
                .WithFooter(footer =>
                {
                    footer.WithText($"⏰ {message.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    footer.WithIconUrl("https://imgur.com/GRbgkEk");
                });
            
            embeds = [embedBuilder];
            return true;
        }

        private static string FormatWithKSuffix(long number)
        {
            int length = number.ToString().Length;
            string formattedNumber = length switch
            {
                <= 6 => (number / 1_000M).ToString("0.###") + "K",
                <= 9 => (number / 1_000_000M).ToString("0.###") + "KK",
                _ => (number / 1_000_000_000M).ToString("0.###") + "KKK",
            };
            return formattedNumber.Replace('.', ',');
        }
    }
}