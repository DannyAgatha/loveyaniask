using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Data.Bazaar;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarItemAddEventHandler : IAsyncEventProcessor<BazaarItemAddEvent>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IBazaarManager _bazaarManager;
    private readonly IBazaarService _bazaarService;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly IServerManager _serverManager;

    public BazaarItemAddEventHandler(IBazaarService bazaarService, IBazaarManager bazaarManager, IGameLanguageService languageService, IServerManager serverManager,
        BazaarConfiguration bazaarConfiguration, IGameItemInstanceFactory itemInstanceFactory)
    {
        _bazaarService = bazaarService;
        _bazaarManager = bazaarManager;
        _languageService = languageService;
        _serverManager = serverManager;
        _bazaarConfiguration = bazaarConfiguration;
        _itemInstanceFactory = itemInstanceFactory;
    }

    public async Task HandleAsync(BazaarItemAddEvent e, CancellationToken cancellation)
    {
        if (e.InventoryItem == null || e.InventoryItem.ItemInstance.Amount < e.Amount)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] The item defined is null or doesn't meet the expectations.");
            return;
        }

        if (!e.InventoryItem.ItemInstance.GameItem.IsSoldable || e.InventoryItem.ItemInstance.IsBound
            || e.InventoryItem.ItemInstance.GameItem.ItemType is ItemType.Specialist or ItemType.Quest1 or ItemType.Quest2)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Item that can't be sold is being tried to be sold.");
            return;
        }

        int maximumListedItems = e.UsedMedal ? _bazaarConfiguration.MaximumListedItemsMedal : _bazaarConfiguration.MaximumListedItems;

        ItemInstanceDTO itemCopy = _itemInstanceFactory.CreateDto(e.InventoryItem.ItemInstance);
        itemCopy.Amount = e.Amount;

        await e.Sender.RemoveItemFromInventory(amount: e.Amount, item: e.InventoryItem);
        e.Sender.PlayerEntity.RemoveGold(e.Tax);

        BazaarItemResponse response = null;

        try
        {
            response = await _bazaarService.AddItemToBazaar(new BazaarAddItemRequest
            {
                ChannelId = _serverManager.ChannelId,
                BazaarItemDto = new BazaarItemDTO
                {
                    Amount = itemCopy.Amount,
                    CharacterId = e.Sender.PlayerEntity.Id,
                    ItemInstance = itemCopy,
                    SaleFee = e.UsedMedal ? 0 : (long)(e.PricePerItem * 0.05),
                    ExpiryDate = e.ExpiryDate,
                    DayExpiryAmount = e.DayExpiryAmount,
                    IsPackage = e.IsPackage,
                    PricePerItem = e.PricePerItem,
                    UsedMedal = e.UsedMedal
                },
                OwnerName = e.Sender.PlayerEntity.Name,
                SunkGold = e.Tax,
                MaximumListedItems = maximumListedItems
            });
        }
        catch (Exception ex)
        {
            Log.Error("[BAZAAR_LISTING]", ex);
        }

        if (response == null || response.ResponseType == RpcResponseType.MAINTENANCE_MODE)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE, e.Sender.UserLanguage));
            await e.Sender.AddNewItemToInventory(_itemInstanceFactory.CreateItem(itemCopy), sendGiftIsFull: true);
            await e.Sender.EmitEventAsync(new GenerateGoldEvent(e.Tax, sendMessage: false));
            return;
        }

        if (response.ResponseType == RpcResponseType.SUCCESS)
        {
            string itemName = e.InventoryItem.ItemInstance.GameItem.GetItemName(_languageService, e.Sender.UserLanguage);
            string message = _languageService.GetLanguageFormat(GameDialogKey.BAZAAR_CHATMESSAGE_ITEM_ADDED, e.Sender.UserLanguage, itemName, e.Amount);
            e.Sender.SendChatMessage(message, ChatMessageColorType.Yellow);
            e.Sender.SendMsg(_languageService.GetLanguage(GameDialogKey.BAZAAR_SHOUTMESSAGE_ITEM_ADDED, e.Sender.UserLanguage), MsgMessageType.Middle);
            e.Sender.SendPacket("rc_reg 1");
            await e.Sender.EmitEventAsync(new BazaarItemAddedEvent
            {
                ItemVnum = e.InventoryItem.ItemInstance.ItemVNum,
                Amount = e.Amount,
                PricePerItem = e.PricePerItem,
                ExpiryDate = e.ExpiryDate,
                UsedMedal = e.UsedMedal,
                IsPackage = e.IsPackage,
                Tax = e.Tax
            });

            await e.Sender.EmitEventAsync(new BazaarItemInsertedEvent
            {
                ItemInstance = e.InventoryItem.ItemInstance,
                BazaarItemId = response.BazaarItemDto.Id,
                Price = e.PricePerItem,
                Quantity = e.Amount,
                Taxes = e.Tax
            });
            return;
        }

        e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_MAXIMUM_ITEMS_REACHED, e.Sender.UserLanguage));
        await e.Sender.AddNewItemToInventory(_itemInstanceFactory.CreateItem(itemCopy), sendGiftIsFull: true);
        await e.Sender.EmitEventAsync(new GenerateGoldEvent(e.Tax, sendMessage: false));
    }
}