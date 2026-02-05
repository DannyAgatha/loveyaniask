using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseOpenEventHandler : IAsyncEventProcessor<AccountWarehouseOpenEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public AccountWarehouseOpenEventHandler(IItemsManager itemsManager, IAccountWarehouseManager accountWarehouseManager, IGameLanguageService languageService,
        IGameFeatureToggleManager gameFeatureToggleManager)
    {
        _itemsManager = itemsManager;
        _accountWarehouseManager = accountWarehouseManager;
        _languageService = languageService;
        _gameFeatureToggleManager = gameFeatureToggleManager;
    }

    public async Task HandleAsync(AccountWarehouseOpenEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.Warehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (character.HasShopOpened || character.IsInExchange() || session.IsInSpecialOrHiddenTimeSpace())
        {
            return;
        }

        int capacity = character.WareHouseSize;

        if (capacity == 0)
        {
            // Try find warehouse 
            MapDesignObject warehouse = session.PlayerEntity.Miniland?.MapDesignObjects?.FirstOrDefault(x => x?.InventoryItem?.ItemInstance != null
                && x.InventoryItem.ItemInstance.GameItem.IsWarehouse);
            if (warehouse != null)
            {
                e.Sender.PlayerEntity.WareHouseSize = warehouse.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint;
                capacity = warehouse.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint;
            }
        }

        (IDictionary<short, AccountWarehouseItemDto> accountWarehouseItemDtos, ManagerResponseType? responseType) = await _accountWarehouseManager.GetWarehouse(session.Account.Id);

        if (responseType != ManagerResponseType.Success)
        {
            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
            return;
        }

        session.PlayerEntity.IsWarehouseOpen = true;
        e.Sender.SendWarehouseStashAll(_itemsManager, capacity, accountWarehouseItemDtos?.Values ?? new List<AccountWarehouseItemDto>());
    }
}