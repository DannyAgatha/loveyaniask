using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseShowItemEventHandler : IAsyncEventProcessor<FamilyWarehouseShowItemEvent>
    {
        private readonly ICharacterAlgorithm _algorithm;
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IItemsManager _itemsManager;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseShowItemEventHandler(IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService languageService, IItemsManager itemsManager,
            ICharacterAlgorithm algorithm, IFamilyWarehouseManager familyWarehouseManager, IGameFeatureToggleManager gameFeatureToggleManager)
        {
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _languageService = languageService;
            _itemsManager = itemsManager;
            _algorithm = algorithm;
            _familyWarehouseManager = familyWarehouseManager;
            _gameFeatureToggleManager = gameFeatureToggleManager;
        }

        public async Task HandleAsync(FamilyWarehouseShowItemEvent e, CancellationToken cancellation)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.FamilyWarehouse);
            if (disabled)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
                return;
            }

            IClientSession session = e.Sender;
            IPlayerEntity character = e.Sender.PlayerEntity;

            if (!character.IsInFamily() || !character.IsFamilyWarehouseOpen || session.CantPerformActionOnAct4() || character.HasShopOpened || character.IsInExchange())
            {
                return;
            }

            if (!session.CheckPutWithdrawPermission(FamilyWarehouseAuthorityType.Put))
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_NOT_ENOUGH_PERMISSION, e.Sender.UserLanguage));
                return;
            }

            (FamilyWarehouseItemDto item, ManagerResponseType? response) = await _familyWarehouseManager.GetWarehouseItem(character.Family.Id, e.Slot, character.Id);

            if (response != ManagerResponseType.Success || item?.ItemInstance == null)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(
                    response == ManagerResponseType.Maintenance ? GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE : GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
                return;
            }

            session.SendEInfoPacket(_gameItemInstanceFactory.CreateItem(item.ItemInstance), _itemsManager, _algorithm);
        }
    }
}