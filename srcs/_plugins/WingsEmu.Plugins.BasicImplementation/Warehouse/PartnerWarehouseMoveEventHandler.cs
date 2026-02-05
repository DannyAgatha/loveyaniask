using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class PartnerWarehouseMoveEventHandler : IAsyncEventProcessor<PartnerWarehouseMoveEvent>
{
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IGameItemInstanceFactory _instanceFactory;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public PartnerWarehouseMoveEventHandler(IGameItemInstanceFactory instanceFactory, IGameFeatureToggleManager gameFeatureToggleManager, IGameLanguageService gameLanguageService, GeneralServerConfiguration generalServerConfiguration)
    {
        _instanceFactory = instanceFactory;
        _gameFeatureToggleManager = gameFeatureToggleManager;
        _gameLanguageService = gameLanguageService;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public async Task HandleAsync(PartnerWarehouseMoveEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.PartnerWarehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        short originalSlot = e.OriginalSlot;
        int amount = e.Amount;
        short newSlot = e.NewSlot;

        if (!session.PlayerEntity.IsPartnerWarehouseOpen)
        {
            return;
        }

        if (originalSlot < 0)
        {
            return;
        }

        if (newSlot < 0)
        {
            return;
        }

        if (newSlot >= session.PlayerEntity.GetPartnerWarehouseSlots())
        {
            return;
        }

        if (!session.PlayerEntity.HaveStaticBonus(StaticBonusType.PartnerBackpack))
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
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

        if (originalSlot == newSlot)
        {
            return;
        }

        PartnerWarehouseItem itemToMove = session.PlayerEntity.GetPartnerWarehouseItem(originalSlot);
        if (itemToMove == null)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        if (amount > _generalServerConfiguration.MaxItemAmount)
        {
            return;
        }

        if (amount > itemToMove.ItemInstance.Amount)
        {
            return;
        }

        PartnerWarehouseItem anotherItem = session.PlayerEntity.GetPartnerWarehouseItem(newSlot);
        if (anotherItem == null)
        {
            if (amount == itemToMove.ItemInstance.Amount)
            {
                session.PlayerEntity.RemovePartnerWarehouseItem(itemToMove.Slot);
                session.PlayerEntity.AddPartnerWarehouseItem(itemToMove.ItemInstance, newSlot);
                session.SendRemovePartnerWarehouseItem(originalSlot);

                PartnerWarehouseItem moved = session.PlayerEntity.GetPartnerWarehouseItem(newSlot);
                session.SendAddPartnerWarehouseItem(moved);
                return;
            }

            GameItemInstance newItem = itemToMove.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM
                            ? _instanceFactory.DuplicateItem(itemToMove.ItemInstance)
                            : _instanceFactory.CreateItem(itemToMove.ItemInstance.ItemVNum, amount);
            
            itemToMove.ItemInstance.Amount -= amount;

            session.PlayerEntity.AddPartnerWarehouseItem(newItem, newSlot);
            PartnerWarehouseItem addedItem = session.PlayerEntity.GetPartnerWarehouseItem(newSlot);
            session.SendAddPartnerWarehouseItem(addedItem);
            session.SendAddPartnerWarehouseItem(itemToMove);
            return;
        }

        GameItemInstance movedInstance = itemToMove.ItemInstance;
        GameItemInstance anotherInstance = anotherItem.ItemInstance;

        if (movedInstance.ItemVNum != anotherInstance.ItemVNum || movedInstance.Type != ItemInstanceType.NORMAL_ITEM)
        {
            session.PlayerEntity.RemovePartnerWarehouseItem(itemToMove.Slot);
            session.PlayerEntity.RemovePartnerWarehouseItem(anotherItem.Slot);

            session.PlayerEntity.AddPartnerWarehouseItem(movedInstance, newSlot);
            PartnerWarehouseItem movedItem = session.PlayerEntity.GetPartnerWarehouseItem(newSlot);
            session.SendAddPartnerWarehouseItem(movedItem);

            session.PlayerEntity.AddPartnerWarehouseItem(anotherInstance, originalSlot);
            movedItem = session.PlayerEntity.GetPartnerWarehouseItem(originalSlot);
            session.SendAddPartnerWarehouseItem(movedItem);
            return;
        }

        if (amount + anotherInstance.Amount > _generalServerConfiguration.MaxItemAmount)
        {
            amount = (short)(_generalServerConfiguration.MaxItemAmount - anotherInstance.Amount);
            if (amount == 0)
            {
                return;
            }
        }

        movedInstance.Amount -= amount;
        anotherInstance.Amount += amount;
        if (movedInstance.Amount <= 0)
        {
            session.PlayerEntity.RemovePartnerWarehouseItem(originalSlot);
            session.SendRemovePartnerWarehouseItem(originalSlot);
        }
        else
        {
            session.SendAddPartnerWarehouseItem(itemToMove);
        }

        session.SendAddPartnerWarehouseItem(anotherItem);
    }
}