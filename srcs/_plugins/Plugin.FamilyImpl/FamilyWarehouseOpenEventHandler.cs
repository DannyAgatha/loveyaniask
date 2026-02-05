using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Features;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseOpenEventHandler : IAsyncEventProcessor<FamilyWarehouseOpenEvent>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
        private readonly IItemsManager _itemsManager;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseOpenEventHandler(IGameLanguageService languageService, IItemsManager itemsManager, IFamilyWarehouseManager familyWarehouseManager,
            IGameFeatureToggleManager gameFeatureToggleManager)
        {
            _languageService = languageService;
            _itemsManager = itemsManager;
            _familyWarehouseManager = familyWarehouseManager;
            _gameFeatureToggleManager = gameFeatureToggleManager;
        }

        public async Task HandleAsync(FamilyWarehouseOpenEvent e, CancellationToken cancellation)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.FamilyWarehouse);
            if (disabled)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
                return;
            }

            IClientSession session = e.Sender;
            IPlayerEntity character = e.Sender.PlayerEntity;
            IFamily family = session.PlayerEntity.Family;

            if (session.CantPerformActionOnAct4() || character.HasShopOpened || character.IsInExchange())
            {
                return;
            }

            if (family == null)
            {
                session.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            int capacity = family.GetWarehouseCapacity();

            if (capacity == 0)
            {
                session.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_WAREHOUSE, session.UserLanguage));
                return;
            }

            (IDictionary<short, FamilyWarehouseItemDto> familyWarehouseItemDtos, ManagerResponseType? responseType) = await _familyWarehouseManager.GetWarehouse(family.Id, session.PlayerEntity.Id);

            if (responseType == null)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
                return;
            }

            if (responseType == ManagerResponseType.Success)
            {
                session.PlayerEntity.IsFamilyWarehouseOpen = true;
                session.SendFamilyWarehouseItems(_itemsManager, capacity, familyWarehouseItemDtos?.Values ?? new List<FamilyWarehouseItemDto>());
                return;
            }

            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
        }
    }
}