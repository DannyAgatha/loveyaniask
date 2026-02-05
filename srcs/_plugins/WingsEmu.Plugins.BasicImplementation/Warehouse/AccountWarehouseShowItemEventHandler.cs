using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Account;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseShowItemEventHandler : IAsyncEventProcessor<AccountWarehouseShowItemEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly ICharacterAlgorithm _algorithm;
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;

    public AccountWarehouseShowItemEventHandler(IAccountWarehouseManager accountWarehouseManager, IGameLanguageService languageService, IGameItemInstanceFactory gameItemInstanceFactory,
        IItemsManager itemsManager, ICharacterAlgorithm algorithm, IGameFeatureToggleManager gameFeatureToggleManager)
    {
        _accountWarehouseManager = accountWarehouseManager;
        _languageService = languageService;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemsManager = itemsManager;
        _algorithm = algorithm;
        _gameFeatureToggleManager = gameFeatureToggleManager;
    }

    public async Task HandleAsync(AccountWarehouseShowItemEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.Warehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (!character.IsWarehouseOpen || character.HasShopOpened || character.IsInExchange())
        {
            return;
        }

        (AccountWarehouseItemDto item, ManagerResponseType? response) = await _accountWarehouseManager.GetWarehouseItem(session.Account.Id, e.Slot);

        if (response != ManagerResponseType.Success)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(
                response == ManagerResponseType.Maintenance ? GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE : GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
            return;
        }

        GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem(item.ItemInstance);
        if (itemInstance.GameItem.EquipmentSlot == EquipmentType.Sp)
        {
            if (itemInstance.GameItem.IsPartnerSpecialist)
            {
                session.SendPartnerSpecialistInfo(itemInstance);
            }
            else
            {
                session.SendSpecialistCardInfo(itemInstance, _algorithm);
            }

            return;
        }

        session.SendEInfoPacket(itemInstance, _itemsManager, _algorithm);
    }
}