using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Features;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseMoveItemEventHandler : IAsyncEventProcessor<FamilyWarehouseMoveItemEvent>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseMoveItemEventHandler(IGameLanguageService languageService, IFamilyWarehouseManager familyWarehouseManager, IGameFeatureToggleManager gameFeatureToggleManager)
        {
            _languageService = languageService;
            _familyWarehouseManager = familyWarehouseManager;
            _gameFeatureToggleManager = gameFeatureToggleManager;
        }

        public async Task HandleAsync(FamilyWarehouseMoveItemEvent e, CancellationToken cancellation)
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

            ManagerResponseType? responseType = await _familyWarehouseManager.MoveWarehouseItem(new FamilyWarehouseItemDto
            {
                FamilyId = character.Family.Id,
                Slot = e.OldSlot
            }, e.Amount, e.NewSlot, character.Id);

            if (responseType == ManagerResponseType.Success)
            {
                return;
            }

            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.ACCOUNT_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
        }
    }
}