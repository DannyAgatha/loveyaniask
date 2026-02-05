using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Inventory;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class PartnerWarehouseDepositEventHandler : IAsyncEventProcessor<PartnerWarehouseDepositEvent>
{
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IGameItemInstanceFactory _instanceFactory;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public PartnerWarehouseDepositEventHandler(IGameItemInstanceFactory instanceFactory, IGameFeatureToggleManager gameFeatureToggleManager, IGameLanguageService gameLanguageService, GeneralServerConfiguration generalServerConfiguration)
    {
        _instanceFactory = instanceFactory;
        _gameFeatureToggleManager = gameFeatureToggleManager;
        _gameLanguageService = gameLanguageService;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public async Task HandleAsync(PartnerWarehouseDepositEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.PartnerWarehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        InventoryType inventoryType = e.InventoryType;
        short inventorySlot = e.InventorySlot;
        int amount = e.Amount;
        short slotDestination = e.SlotDestination;

        if (!session.PlayerEntity.HaveStaticBonus(StaticBonusType.PartnerBackpack))
        {
            return;
        }

        if (!session.PlayerEntity.IsPartnerWarehouseOpen)
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (slotDestination >= session.PlayerEntity.GetPartnerWarehouseSlots())
        {
            return;
        }

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(inventorySlot, inventoryType);

        if (item == null)
        {
            return;
        }

        if (item.IsEquipped)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        if (amount > item.ItemInstance.Amount)
        {
            return;
        }

        if (amount > _generalServerConfiguration.MaxItemAmount)
        {
            return;
        }

        PartnerWarehouseItem anotherItem = session.PlayerEntity.GetPartnerWarehouseItem(slotDestination);
        if (anotherItem == null)
        {
            if (!session.PlayerEntity.HasSpaceForPartnerWarehouseItem())
            {
                return;
            }

            GameItemInstance newItem = item.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM
                ? _instanceFactory.DuplicateItem(item.ItemInstance)
                : _instanceFactory.CreateItem(item.ItemInstance.ItemVNum, amount);

            session.PlayerEntity.AddPartnerWarehouseItem(newItem, slotDestination);
            PartnerWarehouseItem partnerWarehouseItem = session.PlayerEntity.GetPartnerWarehouseItem(slotDestination);
            session.SendAddPartnerWarehouseItem(partnerWarehouseItem);
            await session.RemoveItemFromInventory(item: item, amount: amount);
            return;
        }

        GameItemInstance itemInstance = item.ItemInstance;
        GameItemInstance anotherInstance = anotherItem.ItemInstance;

        if (itemInstance.ItemVNum != anotherInstance.ItemVNum)
        {
            return;
        }

        if (itemInstance.Type != ItemInstanceType.NORMAL_ITEM)
        {
            return;
        }

        if (amount + anotherInstance.Amount > _generalServerConfiguration.MaxItemAmount)
        {
            amount = (short)(_generalServerConfiguration.MaxItemAmount  - anotherInstance.Amount);
            if (amount <= 0)
            {
                return;
            }
        }

        anotherInstance.Amount += amount;
        session.SendAddPartnerWarehouseItem(anotherItem);
        await session.RemoveItemFromInventory(item: item, amount: amount);
    }
}