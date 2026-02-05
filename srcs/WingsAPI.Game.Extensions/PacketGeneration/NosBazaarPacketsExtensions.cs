// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class NosBazaarPacketsExtensions
    {
        public static string GenerateBazaarItemsPacket(this IClientSession session) => "rc_blist";

        public static string GenerateRcScalc(bool returned, long pricePerUnit, int soldAmount, int totalAmount, long taxes, long totalProfit, long itemVNum)
            => $"rc_scalc {(returned ? 1 : 0).ToString()} {pricePerUnit.ToString()} {soldAmount.ToString()} {totalAmount.ToString()} {taxes.ToString()} {totalProfit.ToString()} {itemVNum.ToString()}";
        //rc_scalc 1 233 0 1 0 0 2038

        public static string GenerateRcBuy(bool returned, long itemVNum, string seller, int amount, long pricePerUnit, long upgrade, long rarity)
            => $"rc_buy {(returned ? 1 : 0).ToString()} {itemVNum.ToString()} {seller} {amount.ToString()} {pricePerUnit.ToString()} 0 {upgrade.ToString()} {rarity.ToString()}";

        public static string GenerateCharacterBazaarItemsPacket(ushort index, IReadOnlyCollection<BazaarItem> items, BazaarListedItemType filter, IItemsManager itemsManager,
             ICharacterAlgorithm characterAlgorithm, BazaarConfiguration bazaarConfiguration)
        {
            const string header = "rc_slist";
            if (items == null || !items.Any())
            {
                return $"{header} 0  0 ";
            }

            string itemsPacket = string.Empty;
            int ignoreCounter = index * bazaarConfiguration.ItemsPerIndex;
            int sendCounter = 0;
            foreach (BazaarItem bazaarItem in items.OrderByDescending(i => i.BazaarItemDto.Id))
            {
                BazaarListedItemType itemStatus = bazaarItem.BazaarItemDto.GetBazaarItemStatus();
                if (filter != BazaarListedItemType.All && itemStatus != filter)
                {
                    continue;
                }

                if (ignoreCounter > 0)
                {
                    ignoreCounter--;
                    continue;
                }

                if (sendCounter >= bazaarConfiguration.ItemsPerIndex)
                {
                    break;
                }

                sendCounter++;

                string minutesPassed = (itemStatus == BazaarListedItemType.DeadlineExpired || (bazaarItem.BazaarItemDto.Amount - bazaarItem.BazaarItemDto.SoldAmount) == 0
                    ? -1
                    : (int)(bazaarItem.BazaarItemDto.ExpiryDate - DateTime.UtcNow).TotalMinutes).ToString();

                GameItemInstance itemInstance = bazaarItem.Item.Type != ItemInstanceType.NORMAL_ITEM ? bazaarItem.Item : null;
                string eqPacket = itemInstance == null ? string.Empty : itemInstance.GenerateEInfo(itemsManager, characterAlgorithm, 0).Replace("e_info ", string.Empty).Replace(" ", "^");
                itemsPacket +=
                    $" {bazaarItem.BazaarItemDto.Id.ToString()}|{bazaarItem.BazaarItemDto.CharacterId.ToString()}|{bazaarItem.Item.ItemVNum.ToString()}|{bazaarItem.BazaarItemDto.SoldAmount.ToString()}" +
                    $"|{bazaarItem.BazaarItemDto.Amount.ToString()}|{(bazaarItem.BazaarItemDto.IsPackage ? 1 : 0).ToString()}|{bazaarItem.BazaarItemDto.PricePerItem.ToString()}|{((byte)itemStatus).ToString()}" +
                    $"|{minutesPassed}|{(bazaarItem.BazaarItemDto.UsedMedal ? 1 : 0).ToString()}|0|{bazaarItem.Item.Rarity.ToString()}|{bazaarItem.Item.Upgrade.ToString()}" +
                    $"|{itemInstance?.GetCarvedRunesInformation(true)}|0|{eqPacket} ";
            }

            return $"{header} {index.ToString()}  0{itemsPacket}"; //Space removed cause it will be added by the generation of the packet
        }

        public static string GenerateSearchResponseBazaarItemsPacket(int index, IReadOnlyCollection<BazaarItem> items, IItemsManager itemsManager, ICharacterAlgorithm characterAlgorithm,
            BazaarConfiguration bazaarConfiguration)
        {
            const string header = "rc_blist";
            if (items == null || !items.Any())
            {
                return $"{header} 0";
            }

            string itemsPacket = string.Empty;
            foreach (BazaarItem bazaarItem in items)
            {
                string minutesPassed = ((int)(bazaarItem.BazaarItemDto.ExpiryDate - DateTime.UtcNow).TotalMinutes).ToString();

                GameItemInstance itemInstance = bazaarItem.Item.Type != ItemInstanceType.NORMAL_ITEM ? bazaarItem.Item : null;
                string eqPacket = itemInstance == null ? string.Empty : itemInstance.GenerateEInfo(itemsManager, characterAlgorithm, 0).Replace("e_info ", string.Empty).Replace(" ", "^");

                itemsPacket += $" {bazaarItem.BazaarItemDto.Id.ToString()}|{bazaarItem.BazaarItemDto.CharacterId.ToString()}|{bazaarItem.OwnerName}|{bazaarItem.Item.ItemVNum.ToString()}" +
                    $"|{(bazaarItem.BazaarItemDto.Amount - bazaarItem.BazaarItemDto.SoldAmount).ToString()}|{(bazaarItem.BazaarItemDto.IsPackage ? 1 : 0).ToString()}" +
                    $"|{bazaarItem.BazaarItemDto.PricePerItem.ToString()}|{minutesPassed}|2|0|{bazaarItem.Item.Rarity.ToString()}|{bazaarItem.Item.Upgrade.ToString()}" +
                    $"|{itemInstance?.GetCarvedRunesInformation(true)}|0|{eqPacket}";
            }

            return $"rc_blist {index.ToString()}{itemsPacket}"; //Space removed cause it will be added by the generation of the packet
        }

        public static void SendBazaarItems(this IClientSession session) => session.SendPacket(session.GenerateBazaarItemsPacket());

        public static void SendSearchResponseBazaarItems(this IClientSession session, int index, IReadOnlyCollection<BazaarItem> items, IItemsManager itemsManager,
            ICharacterAlgorithm characterAlgorithm, BazaarConfiguration bazaarConfiguration)
            => session.SendPacket(GenerateSearchResponseBazaarItemsPacket(index, items, itemsManager, characterAlgorithm, bazaarConfiguration));

        public static void SendCharacterListedBazaarItems(this IClientSession session, ushort index, IReadOnlyCollection<BazaarItem> items, BazaarListedItemType filter, IItemsManager itemsManager,
            ICharacterAlgorithm characterAlgorithm, BazaarConfiguration bazaarConfiguration)
            => session.SendPacket(GenerateCharacterBazaarItemsPacket(index, items, filter, itemsManager, characterAlgorithm, bazaarConfiguration));

        public static void SendBazaarResponseItemRemove(this IClientSession session, bool returned, long pricePerUnit, int soldAmount, int totalAmount, long taxes, long totalProfit, long itemVNum)
        {
            session.SendPacket(GenerateRcScalc(returned, pricePerUnit, soldAmount, totalAmount, taxes, totalProfit, itemVNum));
        }

        public static void SendBazaarResponseItemBuy(this IClientSession session, bool returned, long itemVNum, string seller, int amount, long pricePerUnit, long upgrade, long rarity)
        {
            session.SendPacket(GenerateRcBuy(returned, itemVNum, seller, amount, pricePerUnit, upgrade, rarity));
        }
    }
}