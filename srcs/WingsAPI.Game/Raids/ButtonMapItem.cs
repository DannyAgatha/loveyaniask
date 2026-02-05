using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Triggers;

namespace WingsEmu.Game.Raids;

public sealed class ButtonMapItem : MapItem, IEventTriggerContainer
{
    private readonly IEventTriggerContainer _eventTriggerContainer;

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="deactivatedStateVNum"></param>
    /// <param name="activatedStateVNum"></param>
    /// <param name="activatedState">
    ///     Initial state of the button, when switching to this state it won't trigger the
    ///     'switchedToNonDefaultState'
    /// </param>
    /// <param name="mapInstance">Map instance in which the button has been added</param>
    /// <param name="asyncEventPipeline"></param>
    /// <param name="isQuest"></param>
    public ButtonMapItem(short x, short y, int deactivatedStateVNum, int activatedStateVNum, bool activatedState, IMapInstance mapInstance, IAsyncEventPipeline asyncEventPipeline,
        bool? onlyOnce = null, bool isObjective = false, bool isQuest = false, int? customDanceDuration = null) : base(x, y, isQuest, mapInstance)
    {
        DeactivatedStateVNum = deactivatedStateVNum;
        ActivatedStateVNum = activatedStateVNum;
        DefaultState = activatedState;
        State = DefaultState;
        IsObjective = isObjective;
        ItemVNum = State ? ActivatedStateVNum : DeactivatedStateVNum;
        _eventTriggerContainer = new EventTriggerContainer(asyncEventPipeline);
        CustomDanceDuration = customDanceDuration;

        Amount = 1;
        CreatedDate = null;
        CanBeMovedOnlyOnce = onlyOnce;
    }

    public bool AlreadyMoved { get; set; }

    public bool IsObjective { get; }

    public int DeactivatedStateVNum { get; }

    public int ActivatedStateVNum { get; }

    public bool DefaultState { get; }

    public bool State { get; set; }

    public bool? CanBeMovedOnlyOnce { get; set; }

    public bool NonDefaultState => State != DefaultState;

    public int? CustomDanceDuration { get; set; }

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false) => _eventTriggerContainer.AddEvent(key, notification, removedOnTrigger);

    public Task TriggerEvents(string key) => _eventTriggerContainer.TriggerEvents(key);

    public override GameItemInstance GetItemInstance() => ItemInstance;
}