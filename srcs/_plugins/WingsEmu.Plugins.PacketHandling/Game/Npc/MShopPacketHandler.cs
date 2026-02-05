using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class MShopPacketHandler : GenericGamePacketHandlerBase<MShopPacket>
{
    private readonly IGameLanguageService _language;

    public MShopPacketHandler(IGameLanguageService language) => _language = language;

    protected override async Task HandlePacketAsync(IClientSession session, MShopPacket packet)
    {
        return;
        if (session.IsActionForbidden())
        {
            return;
        }

        switch (packet.Type)
        {
            case MShopPacketType.CloseShop:
                await session.EmitEventAsync(new ShopPlayerCloseEvent());
                break;
            case MShopPacketType.OpenDialog:
                session.SendPacket("ishop");
                break;

            case MShopPacketType.OpenShop:
                await ProcessPacketStructure(session, packet.PacketData);
                break;

            default:
                return;
        }
    }

    private async Task ProcessPacketStructure(IClientSession session, string packet)
    {
        string[] packetsplit = packet.Split(' ');

        if (packetsplit.Length <= 80)
        {
            return;
        }

        if (session.IsActionForbidden() || session.PlayerEntity.IsInExchange() || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsShopping || session.PlayerEntity.IsWarehouseOpen
            || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen || session.PlayerEntity.HasNosBazaarOpen)
        {
            return;
        }


        var list = new List<ShopPlayerItem>();
        const int amountPersonalShopItems = 20;
        int nonNullItemsCount = 0;

        for (short i = 0; i < amountPersonalShopItems; i++)
        {
            int packetIndexGuide = i * 4;
            if (!(Enum.TryParse(packetsplit[packetIndexGuide], out InventoryType inventoryType)
                    && short.TryParse(packetsplit[packetIndexGuide + 1], out short inventorySlot)
                    && short.TryParse(packetsplit[packetIndexGuide + 2], out short sellAmount)
                    && long.TryParse(packetsplit[packetIndexGuide + 3], out long price)))
            {
                list.Add(null);
                continue;
            }

            if (inventorySlot < 0 || sellAmount < 1 || price < 1)
            {
                list.Add(null);
                continue;
            }

            if (inventoryType != InventoryType.Equipment && inventoryType != InventoryType.Etc && inventoryType != InventoryType.Main)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING,
                    $"[PLAYER_SHOP_OPEN] Tried to add an object from a non allowed InventoryType. InventoryType: '{inventoryType.ToString()}'");
                return;
            }

            InventoryItem inv = session.PlayerEntity.GetItemBySlotAndType(inventorySlot, inventoryType);
            if (inv == null || inv.ItemInstance.Amount < sellAmount)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[PLAYER_SHOP_OPEN] Tried to add a nonexistent object.");
                return;
            }

            if (!inv.ItemInstance.GameItem.IsTradable || inv.ItemInstance.IsBound)
            {
                session.SendChatMessage(_language.GetLanguage(GameDialogKey.SHOP_CHATMESSAGE_ONLY_TRADABLE_ITEMS, session.UserLanguage), ChatMessageColorType.Yellow);
                session.SendShopEndPacket(ShopEndType.Player);
                return;
            }

            list.Add(new ShopPlayerItem
            {
                ShopSlot = i,
                PricePerUnit = price,
                InventoryType = inventoryType,
                InventorySlot = inventorySlot,
                SellAmount = sellAmount
            });
            nonNullItemsCount++;
        }

        if (nonNullItemsCount < 1)
        {
            session.SendShopEndPacket(ShopEndType.Player);
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.SHOP_CHATMESSAGE_EMPTY, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        var shopNameBuilder = new StringBuilder();

        const int maxShopNameLength = 20;
        for (int j = 80; j < packetsplit.Length; j++)
        {
            if (maxShopNameLength < shopNameBuilder.Length)
            {
                break;
            }

            shopNameBuilder.Append(packetsplit[j]);
            shopNameBuilder.Append(' ');
        }

        string shopName = shopNameBuilder.ToString(0, maxShopNameLength < shopNameBuilder.Length ? maxShopNameLength : shopNameBuilder.Length);

        if (string.IsNullOrWhiteSpace(shopName) || string.IsNullOrEmpty(shopName))
        {
            shopName = _language.GetLanguageFormat(GameDialogKey.SHOP_DEFAULT_NAME, session.UserLanguage, session.PlayerEntity.Name);
        }

        await session.EmitEventAsync(new ShopPlayerOpenEvent
        {
            Items = list,
            ShopTitle = shopName
        });
    }
}