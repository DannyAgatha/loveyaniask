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
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseAddEventHandler : IAsyncEventProcessor<AccountWarehouseAddItemEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public AccountWarehouseAddEventHandler(IGameItemInstanceFactory gameItemInstanceFactory, IAccountWarehouseManager accountWarehouseManager, IGameLanguageService languageService,
        IItemsManager itemsManager, IGameFeatureToggleManager gameFeatureToggleManager)
    {
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _accountWarehouseManager = accountWarehouseManager;
        _languageService = languageService;
        _itemsManager = itemsManager;
        _gameFeatureToggleManager = gameFeatureToggleManager;
    }

    public async Task HandleAsync(AccountWarehouseAddItemEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.Warehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;
        InventoryItem inventoryItem = e.Item;

        if (!character.IsWarehouseOpen || character.HasShopOpened || character.IsInExchange()
            || inventoryItem.ItemInstance.Amount < e.Amount || e.SlotDestination >= character.WareHouseSize
            || inventoryItem.ItemInstance.GameItem.IsLimited)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.ACCOUNT_WAREHOUSE_INVALID_ITEM, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        ItemInstanceDTO mapped = _gameItemInstanceFactory.CreateDto(inventoryItem.ItemInstance);
        mapped.Amount = e.Amount;

        await e.Sender.RemoveItemFromInventory(amount: e.Amount, item: inventoryItem);

        (AccountWarehouseItemDto updatedItem, ManagerResponseType? response) = await _accountWarehouseManager.AddWarehouseItem(new AccountWarehouseItemDto
        {
            AccountId = session.Account.Id,
            ItemInstance = mapped,
            Slot = e.SlotDestination
        });

        if (response == ManagerResponseType.Success)
        {
            e.Sender.SendStashPacket(_itemsManager, updatedItem);
            await e.Sender.EmitEventAsync(new WarehouseItemPlacedEvent
            {
                ItemInstance = mapped,
                Amount = e.Amount,
                DestinationSlot = e.SlotDestination
            });
            return;
        }

        await e.Sender.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(mapped), sendGiftIsFull: true);
        e.Sender.SendInfo(response == ManagerResponseType.Maintenance
            ? _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
            : _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
    }
}