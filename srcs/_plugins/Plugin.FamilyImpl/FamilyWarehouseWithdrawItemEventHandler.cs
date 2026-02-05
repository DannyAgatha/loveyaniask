using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseWithdrawItemEventHandler : IAsyncEventProcessor<FamilyWarehouseWithdrawItemEvent>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseWithdrawItemEventHandler(IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService languageService, IFamilyWarehouseManager familyWarehouseManager,
            IGameFeatureToggleManager gameFeatureToggleManager)
        {
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _languageService = languageService;
            _familyWarehouseManager = familyWarehouseManager;
            _gameFeatureToggleManager = gameFeatureToggleManager;
        }

        public async Task HandleAsync(FamilyWarehouseWithdrawItemEvent e, CancellationToken cancellation)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.FamilyWarehouse);
            if (disabled)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
                return;
            }

            IClientSession session = e.Sender;
            IPlayerEntity character = session.PlayerEntity;

            if (!character.IsInFamily() || !character.IsFamilyWarehouseOpen || session.CantPerformActionOnAct4() || character.HasShopOpened || character.IsInExchange())
            {
                return;
            }

            long familyId = character.Family.Id;

            if (!session.CheckPutWithdrawPermission(FamilyWarehouseAuthorityType.PutAndWithdraw))
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_NOT_ENOUGH_PERMISSION, e.Sender.UserLanguage));
                return;
            }

            (FamilyWarehouseItemDto itemToWithdraw, ManagerResponseType? responseType) = await _familyWarehouseManager.GetWarehouseItem(familyId, e.Slot, character.Id);

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

            (ItemInstanceDTO withdrawnItem, ManagerResponseType? responseType2) = await _familyWarehouseManager.WithdrawWarehouseItem(itemToWithdraw, e.Amount, character.Id, character.Name);

            if (responseType2 == ManagerResponseType.Success)
            {
                await session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(withdrawnItem), sendGiftIsFull: true);
                await session.EmitEventAsync(new FamilyWarehouseItemWithdrawnEvent
                {
                    ItemInstance = withdrawnItem,
                    Amount = e.Amount,
                    FromSlot = e.Slot
                });
                return;
            }

            e.Sender.SendInfo(responseType2 == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
        }
    }
}