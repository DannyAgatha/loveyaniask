using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Scripting.Object.Timespace;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpacePickUpItemEventHandler : IAsyncEventProcessor<TimeSpacePickUpItemEvent>
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandler;
    private readonly IChestDropItemConfig _chestDropItemConfig;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly IRandomGenerator _randomGenerator;

    public TimeSpacePickUpItemEventHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager,
        IGameItemInstanceFactory gameItemInstanceFactory, IRandomGenerator randomGenerator, IChestDropItemConfig chestDropItemConfig,
        IBCardEffectHandlerContainer bCardEffectHandler)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _randomGenerator = randomGenerator;
        _chestDropItemConfig = chestDropItemConfig;
        _bCardEffectHandler = bCardEffectHandler;
    }

    public async Task HandleAsync(TimeSpacePickUpItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        TimeSpaceMapItem item = e.TimeSpaceMapItem;
        IMateEntity mateEntity = e.MateEntity;
        GameItemInstance mapItemInstance = item.GetItemInstance();

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        short? objectiveItem = timeSpace.Instance.TimeSpaceObjective.CollectItemVnum;
        if (item.IsObjective && objectiveItem.HasValue && item.ItemVNum == objectiveItem.Value)
        {
            timeSpace.Instance.TimeSpaceObjective.CollectedItemAmount++;
            await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent
            {
                MapInstanceId = session.CurrentMapInstance.Id
            });
        }

        if (mapItemInstance.GameItem.IsTimeSpaceChest())
        {
            switch (mapItemInstance.GameItem.Data[0])
            {
                case 4:

                    ChestDropItemConfiguration config = _chestDropItemConfig.GetChestByDataId(mapItemInstance.GameItem.Data[2]);

                    if (config?.PossibleItems == null)
                    {
                        break;
                    }

                    if (_randomGenerator.RandomNumber() > config.ItemChance)
                    {
                        session.SendMsg(session.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CHEST_EMPTY), MsgMessageType.Middle);
                        break;
                    }

                    ChestDropItemDrop getRandomItem = config.PossibleItems[_randomGenerator.RandomNumber(config.PossibleItems.Count)];
                    if (getRandomItem == null)
                    {
                        break;
                    }

                    GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(getRandomItem.ItemVnum, getRandomItem.Amount);
                    await session.AddNewItemToInventory(newItem, sendGiftIsFull: true);

                    string itemName = newItem.GameItem.GetItemName(_gameLanguage, session.UserLanguage);
                    session.SendMsg(session.GetLanguageFormat(GameDialogKey.INVENTORY_CHATMESSAGE_X_ITEM_ACQUIRED, getRandomItem.Amount, itemName), MsgMessageType.Middle);
                    break;
            }
        }

        session.CurrentMapInstance.RemoveDrop(item.TransportId);
        await item.TriggerEvents(TimespaceConstEventKeys.PickedUp);

        if (mateEntity == null)
        {
            session.BroadcastGetPacket(item.TransportId);
        }
        else
        {
            mateEntity.BroadcastMateGetPacket(item.TransportId);
            mateEntity.Owner.Session.SendCondMate(mateEntity);
            mateEntity.Owner?.Session.SendPacket(mateEntity.GenerateEffectPacket(EffectType.PetPickUp));
        }
    }
}