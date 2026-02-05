using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Items;
using WingsEmu.Game.Triggers;

namespace WingsEmu.Game.Maps;

public class TimeSpaceMapItem : MapItem, IEventTriggerContainer
{
    private readonly IEventTriggerContainer _eventTriggerContainer;

    public TimeSpaceMapItem(short x, short y, bool isQuest, GameItemInstance gameItemInstance, IAsyncEventPipeline asyncEventPipeline, IMapInstance mapInstance, int? dancingTime, bool isObjective)
        : base(x, y, isQuest, mapInstance)
    {
        ItemInstance = gameItemInstance;
        _eventTriggerContainer = new EventTriggerContainer(asyncEventPipeline);
        DancingTime = dancingTime;
        IsObjective = isObjective;
        Amount = 1;
        ItemVNum = gameItemInstance.ItemVNum;
        CreatedDate = null;
    }

    public int? DancingTime { get; }
    public bool IsObjective { get; }

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false)
    {
        _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);
    }

    public Task TriggerEvents(string key) => _eventTriggerContainer.TriggerEvents(key);

    public override GameItemInstance GetItemInstance() => ItemInstance;
}