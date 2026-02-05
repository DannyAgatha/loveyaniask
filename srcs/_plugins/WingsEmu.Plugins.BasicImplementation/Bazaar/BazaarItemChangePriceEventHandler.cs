using PhoenixLib.Events;
using PhoenixLib.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarItemChangePriceEventHandler : IAsyncEventProcessor<BazaarItemChangePriceEvent>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IBazaarManager _bazaarManager;
    private readonly IBazaarService _bazaarService;
    private readonly IGameLanguageService _languageService;
    private readonly IServerManager _serverManager;
    private readonly IAccountService _accountService;
    public BazaarItemChangePriceEventHandler(IBazaarManager bazaarManager, IBazaarService bazaarService, IServerManager serverManager, IGameLanguageService languageService,
        BazaarConfiguration bazaarConfiguration, IAccountService accountService)
    {
        _bazaarManager = bazaarManager;
        _bazaarService = bazaarService;
        _serverManager = serverManager;
        _languageService = languageService;
        _bazaarConfiguration = bazaarConfiguration;
        _accountService = accountService;
    }

    public async Task HandleAsync(BazaarItemChangePriceEvent e, CancellationToken cancellation)
    {
        await MainMethod(e);

        await e.Sender.EmitEventAsync(new BazaarGetListedItemsEvent(0, BazaarListedItemType.All));
    }

    private async Task MainMethod(BazaarItemChangePriceEvent e)
    {
        BazaarItem cachedItem = await _bazaarManager.GetBazaarItemById(e.BazaarItemId);
        if (cachedItem == null)
        {
            return;
        }

        if (cachedItem.BazaarItemDto.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING,
                $"Tried to change the price of an item that the character doesn't own. BazaarItemId: {cachedItem.BazaarItemDto.Id.ToString()}");
            
            var banRequest = new BanAccountRequest
            {
                AccountId = e.Sender.Account.Id,
                Reason = "Tried to change the price of an item that the character doesn't own."
            };

            AccountBanSaveResponse banResponse = await _accountService.BanAccount(banRequest);

            if (banResponse.ResponseType != RpcResponseType.SUCCESS)
            {
                return;
            }
                    
            Log.Info($"Account ID [{e.Sender.Account.Id}] has been banned for ried to change the price of an item that the character doesn't own.");
            e.Sender.ForceDisconnect();
            return;
        }

        if (cachedItem.BazaarItemDto.SoldAmount > 0 || cachedItem.BazaarItemDto.GetBazaarItemStatus() != BazaarListedItemType.ForSale)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_ITEM_CHANGED, e.Sender.UserLanguage));
            return;
        }

        if (BazaarExtensions.PriceOrAmountExceeds(cachedItem.BazaarItemDto.UsedMedal, e.NewPricePerItem, cachedItem.BazaarItemDto.Amount))
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_PRICE_EXCEEDS_LIMITS, e.Sender.UserLanguage));
            return;
        }

        DateTime currentDate = DateTime.UtcNow;
        if (e.Sender.PlayerEntity.LastAdministrationBazaarRefresh > currentDate)
        {
            return;
        }

        e.Sender.PlayerEntity.LastAdministrationBazaarRefresh = currentDate.AddSeconds(_bazaarConfiguration.DelayServerBetweenRequestsInSecs);

        BazaarItemResponse response = null;
        try
        {
            response = await _bazaarService.ChangeItemPriceFromBazaar(new BazaarChangeItemPriceRequest
            {
                ChannelId = _serverManager.ChannelId,
                BazaarItemDto = cachedItem.BazaarItemDto,
                ChangerCharacterId = e.Sender.PlayerEntity.Id,
                NewPrice = e.NewPricePerItem,
                NewSaleFee = cachedItem.BazaarItemDto.UsedMedal ? 0 : (long)(e.NewPricePerItem * 0.05)
            });
        }
        catch (Exception ex)
        {
            Log.Error(nameof(BazaarItemChangePriceEventHandler), ex);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            e.Sender.SendInfo(
                response?.ResponseType == RpcResponseType.MAINTENANCE_MODE
                    ? _languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE, e.Sender.UserLanguage)
                    : _languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_RESYNC, e.Sender.UserLanguage));

            return;
        }

        e.Sender.SendMsg(_languageService.GetLanguage(GameDialogKey.BAZAAR_SHOUTMESSAGE_ITEM_PRICE_CHANGED, e.Sender.UserLanguage), MsgMessageType.Middle);
    }
}