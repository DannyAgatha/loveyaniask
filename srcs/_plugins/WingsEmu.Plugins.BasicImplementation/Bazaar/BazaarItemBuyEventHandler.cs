using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Language;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarItemBuyEventHandler : IAsyncEventProcessor<BazaarItemBuyEvent>
{
    private readonly IBazaarManager _bazaarManager;
    private readonly IBazaarService _bazaarService;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<BazaarNotificationMessage> _messagePublisher;
    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;

    public BazaarItemBuyEventHandler(IBazaarManager bazaarManager, IBazaarService bazaarService,
        IGameLanguageService languageService, IServerManager serverManager, IGameItemInstanceFactory itemInstanceFactory,
        ISessionManager sessionManager, IMessagePublisher<BazaarNotificationMessage> messagePublisher)
    {
        _bazaarManager = bazaarManager;
        _bazaarService = bazaarService;
        _languageService = languageService;
        _serverManager = serverManager;
        _itemInstanceFactory = itemInstanceFactory;
        _sessionManager = sessionManager;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(BazaarItemBuyEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsInExchange())
        {
            return;
        }
        
        if (e.Sender.PlayerEntity.IsShopping)
        {
            return;
        }

        long cost = e.PricePerItem * e.Amount;

        if (!e.Sender.HasEnoughGold(cost))
        {
            e.Sender.SendInfoI(Game18NConstString.NotEnoughGold5);
            return;
        }

        e.Sender.PlayerEntity.RemoveGold(cost);
        
        if (cost >= 100000000)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING,
                $" {e.Sender.PlayerEntity.Name} Buyed a bazaar item cost {cost}");
        }

        BazaarItemResponse response = null;
        try
        {
            response = await _bazaarService.BuyItemFromBazaar(new BazaarBuyItemRequest
            {
                ChannelId = _serverManager.ChannelId,
                BazaarItemId = e.BazaarItemId,
                BuyerCharacterId = e.Sender.PlayerEntity.Id,
                Amount = e.Amount,
                PricePerItem = e.PricePerItem
            });
        }
        catch (Exception ex)
        {
            Log.Error("", ex);
        }

        switch (response?.ResponseType)
        {
            case RpcResponseType.MAINTENANCE_MODE:
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE, e.Sender.UserLanguage));
                break;
            case RpcResponseType.SUCCESS:
                GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(response.BazaarItemDto.ItemInstance);
                itemInstance.Amount = e.Amount;
                itemInstance.PityCounter.Clear();
                await e.Sender.AddNewItemToInventory(itemInstance, sendGiftIsFull: true);
                string ownerName = await _bazaarManager.GetOwnerName(response.BazaarItemDto.CharacterId);
                await e.Sender.EmitEventAsync(new BazaarItemBoughtEvent
                {
                    BoughtItem = itemInstance,
                    BazaarItemId = response.BazaarItemDto.Id,
                    Amount = e.Amount,
                    PricePerItem = e.PricePerItem,
                    SellerId = response.BazaarItemDto.CharacterId,
                    SellerName = ownerName
                });
                e.Sender.SendBazaarResponseItemBuy(true, itemInstance.ItemVNum, ownerName, e.Amount, e.PricePerItem,
                    itemInstance.Upgrade, itemInstance.Rarity);
                await SendNotificationToOwner(ownerName, itemInstance);
                return;
            default:
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_ITEM_CHANGED, e.Sender.UserLanguage));
                break;
        }

        await e.Sender.EmitEventAsync(new GenerateGoldEvent(cost, sendMessage: false));
        e.Sender.SendBazaarResponseItemBuy(false, 0, "", 0, 0, 0, 0);
    }

    private async Task SendNotificationToOwner(string ownerName, GameItemInstance itemInstance)
    {
        IClientSession owner = _sessionManager.GetSessionByCharacterName(ownerName);
        if (owner != null)
        {
            string itemName = itemInstance.GameItem.GetItemName(_languageService, owner.UserLanguage);
            int amount = itemInstance.Amount;

            owner.SendChatMessage(owner.GetLanguageFormat(GameDialogKey.BAZAAR_CHATMESSAGE_ITEM_SOLD, amount, itemName), ChatMessageColorType.Green);
            return;
        }

        await _messagePublisher.PublishAsync(new BazaarNotificationMessage
        {
            OwnerName = ownerName,
            ItemVnum = itemInstance.ItemVNum,
            Amount = itemInstance.Amount
        });
    }
}