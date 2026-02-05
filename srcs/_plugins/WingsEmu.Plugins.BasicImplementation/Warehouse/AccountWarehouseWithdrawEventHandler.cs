using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseWithdrawEventHandler : IAsyncEventProcessor<AccountWarehouseWithdrawItemEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public AccountWarehouseWithdrawEventHandler(IGameLanguageService languageService, IGameItemInstanceFactory gameItemInstanceFactory, IAccountWarehouseManager accountWarehouseManager,
        IItemsManager itemsManager, IGameFeatureToggleManager gameFeatureToggleManager)
    {
        _languageService = languageService;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _accountWarehouseManager = accountWarehouseManager;
        _itemsManager = itemsManager;
        _gameFeatureToggleManager = gameFeatureToggleManager;
    }

    public async Task HandleAsync(AccountWarehouseWithdrawItemEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.Warehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;

        if (!character.IsWarehouseOpen || character.HasShopOpened || character.IsInExchange())
        {
            return;
        }

        (AccountWarehouseItemDto itemToWithdraw, ManagerResponseType? responseType) = await _accountWarehouseManager.GetWarehouseItem(session.Account.Id, e.Slot);

        if (responseType == null)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
            return;
        }

        if (itemToWithdraw?.ItemInstance == null)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
            return;
        }

        if (!character.HasSpaceFor(itemToWithdraw.ItemInstance.ItemVNum, (short)itemToWithdraw.ItemInstance.Amount))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), MsgMessageType.Middle);
            return;
        }

        if (responseType != ManagerResponseType.Success)
        {
            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));

            return;
        }

        (AccountWarehouseItemDto updatedItem, ItemInstanceDTO withdrawnItem, ManagerResponseType? responseType2) = await _accountWarehouseManager.WithdrawWarehouseItem(itemToWithdraw, e.Amount);

        if (responseType2 is ManagerResponseType.Success)
        {
            session.SendStashDynamicItemUpdate(_itemsManager, updatedItem, e.Slot);
            await session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(withdrawnItem), sendGiftIsFull: true);
            await session.EmitEventAsync(new WarehouseItemWithdrawnEvent
            {
                ItemInstance = withdrawnItem,
                Amount = withdrawnItem.Amount,
                FromSlot = e.Slot
            });
            return;
        }

        e.Sender.SendInfo(responseType2 is ManagerResponseType.Maintenance
            ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
            : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
    }
}