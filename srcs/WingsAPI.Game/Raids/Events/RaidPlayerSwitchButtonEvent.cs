using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPlayerSwitchButtonEvent : PlayerEvent
{
    public RaidPlayerSwitchButtonEvent(ButtonMapItem buttonMapItem) => ButtonMapItem = buttonMapItem;

    public ButtonMapItem ButtonMapItem { get; }
}