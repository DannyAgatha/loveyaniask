using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class PartnerWarehouseWithdrawEventHandler : IAsyncEventProcessor<PartnerWarehouseWithdrawEvent>
{
    private readonly IGameFeatureToggleManager _gameFeatureToggleManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public PartnerWarehouseWithdrawEventHandler(IGameLanguageService gameLanguage, IGameItemInstanceFactory gameItemInstanceFactory, IGameFeatureToggleManager gameFeatureToggleManager, GeneralServerConfiguration generalServerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _gameFeatureToggleManager = gameFeatureToggleManager;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public async Task HandleAsync(PartnerWarehouseWithdrawEvent e, CancellationToken cancellation)
    {
        bool disabled = await _gameFeatureToggleManager.IsDisabled(GameFeature.PartnerWarehouse);
        if (disabled)
        {
            e.Sender.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.GAME_FEATURE_DISABLED, e.Sender.UserLanguage));
            return;
        }

        IClientSession session = e.Sender;
        short slot = e.Slot;
        int amount = e.Amount;

        if (slot < 0)
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

        if (slot >= session.PlayerEntity.GetPartnerWarehouseSlotsWithoutBackpack())
        {
            return;
        }

        PartnerWarehouseItem item = session.PlayerEntity.GetPartnerWarehouseItem(slot);
        if (item == null)
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

        if (!session.PlayerEntity.HasSpaceFor(item.ItemInstance.ItemVNum, amount))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), ChatMessageColorType.Yellow);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        GameItemInstance itemInstance = item.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM
                    ? _gameItemInstanceFactory.DuplicateItem(item.ItemInstance)
                    : _gameItemInstanceFactory.CreateItem(item.ItemInstance.ItemVNum, amount);
        item.ItemInstance.Amount -= amount;
        if (item.ItemInstance.Amount <= 0)
        {
            session.PlayerEntity.RemovePartnerWarehouseItem(slot);
            session.SendRemovePartnerWarehouseItem(slot);
        }
        else
        {
            session.SendAddPartnerWarehouseItem(item);
        }

        await session.AddNewItemToInventory(itemInstance);
    }
}