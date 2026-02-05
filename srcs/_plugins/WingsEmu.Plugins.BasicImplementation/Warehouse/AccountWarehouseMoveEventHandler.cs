using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseMoveEventHandler : IAsyncEventProcessor<AccountWarehouseMoveEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public AccountWarehouseMoveEventHandler(IAccountWarehouseManager accountWarehouseManager, IGameLanguageService languageService, IItemsManager itemsManager,
        IGameFeatureToggleManager gameFeatureToggleManager)
    {
        _accountWarehouseManager = accountWarehouseManager;
        _languageService = languageService;
        _itemsManager = itemsManager;
        _gameFeatureToggleManager = gameFeatureToggleManager;
    }

    public async Task HandleAsync(AccountWarehouseMoveEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.Warehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsWarehouseOpen || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange()
            || e.OriginalSlot < 0 || e.NewSlot < 0 || e.OriginalSlot == e.NewSlot || e.NewSlot >= session.PlayerEntity.WareHouseSize)
        {
            return;
        }

        (AccountWarehouseItemDto oldItem, AccountWarehouseItemDto newItem, ManagerResponseType? responseType) = await _accountWarehouseManager.MoveWarehouseItem(new AccountWarehouseItemDto
        {
            AccountId = session.PlayerEntity.AccountId,
            Slot = e.OriginalSlot
        }, e.Amount, e.NewSlot);

        if (responseType != ManagerResponseType.Success)
        {
            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
            return;
        }

        session.SendStashDynamicItemUpdate(_itemsManager, oldItem, e.OriginalSlot);
        session.SendStashDynamicItemUpdate(_itemsManager, newItem, e.NewSlot);
    }
}