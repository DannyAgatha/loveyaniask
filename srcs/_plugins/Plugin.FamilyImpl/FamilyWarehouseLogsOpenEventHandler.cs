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
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseLogsOpenEventHandler : IAsyncEventProcessor<FamilyWarehouseLogsOpenEvent>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseLogsOpenEventHandler(IGameLanguageService languageService, IFamilyWarehouseManager familyWarehouseManager)
        {
            _languageService = languageService;
            _familyWarehouseManager = familyWarehouseManager;
        }

        public async Task HandleAsync(FamilyWarehouseLogsOpenEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IPlayerEntity character = e.Sender.PlayerEntity;
            IFamily family = session.PlayerEntity.Family;

            if (e.Refresh && !character.IsFamilyWarehouseLogsOpen || character.IsFamilyWarehouseOpen || session.CantPerformActionOnAct4() || character.HasShopOpened || character.IsInExchange())
            {
                return;
            }

            if (family == null)
            {
                session.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (family.GetWarehouseCapacity() == 0)
            {
                session.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_WAREHOUSE, session.UserLanguage));
                return;
            }

            if (!session.CheckLogHistoryPermission())
            {
                session.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_NOT_ENOUGH_PERMISSION, session.UserLanguage));
                return;
            }

            (IList<FamilyWarehouseLogEntryDto> logs, ManagerResponseType? responseType) = await _familyWarehouseManager.GetWarehouseLogs(family.Id, character.Id);

            if (responseType == null)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
                return;
            }

            if (responseType == ManagerResponseType.Success)
            {
                session.PlayerEntity.IsFamilyWarehouseLogsOpen = true;
                session.SendFamilyWarehouseLogs(logs ?? new List<FamilyWarehouseLogEntryDto>());
                return;
            }

            e.Sender.SendInfo(responseType == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
        }
    }
}