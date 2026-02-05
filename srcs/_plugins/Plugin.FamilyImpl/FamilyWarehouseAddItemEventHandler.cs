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
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseAddItemEventHandler : IAsyncEventProcessor<FamilyWarehouseAddItemEvent>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IGameLanguageService _languageService;

        public FamilyWarehouseAddItemEventHandler(IGameLanguageService languageService, IGameItemInstanceFactory gameItemInstanceFactory, IFamilyWarehouseManager familyWarehouseManager,
            IGameFeatureToggleManager gameFeatureToggleManager)
        {
            _languageService = languageService;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _familyWarehouseManager = familyWarehouseManager;
            _gameFeatureToggleManager = gameFeatureToggleManager;
        }

        public async Task HandleAsync(FamilyWarehouseAddItemEvent e, CancellationToken cancellation)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.FamilyWarehouse);
            if (disabled)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
                return;
            }

            IClientSession session = e.Sender;
            IPlayerEntity character = session.PlayerEntity;
            InventoryItem inventoryItem = e.Item;

            if (!character.IsInFamily() || !character.IsFamilyWarehouseOpen || session.CantPerformActionOnAct4() || character.HasShopOpened || character.IsInExchange()
                || inventoryItem.ItemInstance.Amount < e.Amount)
            {
                return;
            }

            if (!session.CheckPutWithdrawPermission(FamilyWarehouseAuthorityType.Put))
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_NOT_ENOUGH_PERMISSION, e.Sender.UserLanguage));
                return;
            }

            if (!inventoryItem.ItemInstance.GameItem.IsSoldable || !inventoryItem.ItemInstance.GameItem.IsTradable || inventoryItem.ItemInstance.IsBound
                || inventoryItem.ItemInstance.GameItem.ItemType is ItemType.Specialist or ItemType.Quest1 or ItemType.Quest2)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_INVALID_ITEM, e.Sender.UserLanguage));
                return;
            }

            ItemInstanceDTO mapped = _gameItemInstanceFactory.CreateDto(inventoryItem.ItemInstance);
            mapped.Amount = e.Amount;

            await e.Sender.RemoveItemFromInventory(amount: e.Amount, item: inventoryItem);

            ManagerResponseType? response = await _familyWarehouseManager.AddWarehouseItem(new FamilyWarehouseItemDto
            {
                FamilyId = character.Family.Id,
                ItemInstance = mapped,
                Slot = e.DestinationSlot
            }, character.Id, character.Name);

            if (response == ManagerResponseType.Success)
            {
                await session.EmitEventAsync(new FamilyWarehouseItemPlacedEvent
                {
                    ItemInstance = mapped,
                    Amount = mapped.Amount,
                    DestinationSlot = e.DestinationSlot
                });
                return;
            }

            await e.Sender.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(mapped), sendGiftIsFull: true);
            e.Sender.SendInfo(response == ManagerResponseType.Maintenance
                ? _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE, e.Sender.UserLanguage)
                : _languageService.GetLanguage(GameDialogKey.FAMILY_INFO_WAREHOUSE_UNEXPECTED_ERROR, e.Sender.UserLanguage));
        }
    }
}