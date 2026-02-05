using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarItemRemoveEventHandler : IAsyncEventProcessor<BazaarItemRemoveEvent>
{
    private readonly IBazaarManager _bazaarManager;
    private readonly IBazaarService _bazaarService;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly IServerManager _serverManager;

    public BazaarItemRemoveEventHandler(IBazaarManager bazaarManager, IBazaarService bazaarService, IServerManager serverManager, IGameLanguageService languageService,
        IGameItemInstanceFactory itemInstanceFactory)
    {
        _bazaarManager = bazaarManager;
        _bazaarService = bazaarService;
        _serverManager = serverManager;
        _languageService = languageService;
        _itemInstanceFactory = itemInstanceFactory;
    }

    public async Task HandleAsync(BazaarItemRemoveEvent e, CancellationToken cancellation)
    {
        BazaarItem item = await _bazaarManager.GetBazaarItemById(e.BazaarItemId);
        if (item == null)
        {
            return;
        }

        if (item.BazaarItemDto.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"Tried to remove an item that is not listed by this character. BazaarItemId: {e.BazaarItemId.ToString()}");
            return;
        }

        int amount = item.BazaarItemDto.Amount - item.BazaarItemDto.SoldAmount;

        if (amount > 0 && !e.Sender.PlayerEntity.HasSpaceFor(item.Item.ItemVNum, (short)amount))
        {
            e.Sender.SendMsg(_languageService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, e.Sender.UserLanguage), MsgMessageType.Middle);
            return;
        }

        BazaarItemResponse response = null;
        try
        {
            response = await _bazaarService.RemoveItemFromBazaar(new BazaarRemoveItemRequest
            {
                ChannelId = _serverManager.ChannelId,
                BazaarItemDto = item.BazaarItemDto,
                RequesterCharacterId = e.Sender.PlayerEntity.Id
            });
        }
        catch (Exception ex)
        {
            Log.Error("", ex);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(
                response?.ResponseType == RpcResponseType.MAINTENANCE_MODE ? GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE : GameDialogKey.BAZAAR_INFO_ITEM_CHANGED, e.Sender.UserLanguage));
            return;
        }

        long pricePerItem = response.BazaarItemDto.PricePerItem;
        long totalProfit = 0;
        long taxes = 0;
        if (response.BazaarItemDto.SoldAmount >= 1)
        {
            taxes = response.BazaarItemDto.SaleFee * response.BazaarItemDto.SoldAmount / response.BazaarItemDto.Amount;
            totalProfit = pricePerItem * response.BazaarItemDto.SoldAmount - taxes;
            await e.Sender.EmitEventAsync(new GenerateGoldEvent(totalProfit, applyGoldMultiplier: false, sendMessage: false, fallBackToBank: true));
            await e.Sender.EmitEventAsync(new BazaarItemRemovedEvent
            {
                ItemVnum = response.BazaarItemDto.ItemInstance.ItemVNum,
                SoldAmount = (short)response.BazaarItemDto.SoldAmount,
                Amount = (short)response.BazaarItemDto.Amount,
                TotalProfit = totalProfit
            });
            await e.Sender.EmitEventAsync(new BazaarItemWithdrawnEvent
            {
                ItemInstance = response.BazaarItemDto.ItemInstance,
                Quantity = response.BazaarItemDto.Amount,
                Price = response.BazaarItemDto.PricePerItem,
                BazaarItemId = response.BazaarItemDto.Id
            });
        }

        GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(response.BazaarItemDto.ItemInstance);
        if (response.BazaarItemDto.SoldAmount < response.BazaarItemDto.Amount)
        {
            itemInstance.Amount = response.BazaarItemDto.Amount - response.BazaarItemDto.SoldAmount;
            await e.Sender.AddNewItemToInventory(itemInstance, sendGiftIsFull: true);
        }

        e.Sender.SendBazaarResponseItemRemove(true, pricePerItem, response.BazaarItemDto.SoldAmount, response.BazaarItemDto.Amount, taxes, totalProfit, itemInstance.ItemVNum);
    }
}