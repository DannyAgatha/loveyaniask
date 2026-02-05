using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class RmvObjMinilandEvent : PlayerEvent
{
    public RmvObjMinilandEvent(short slot) => Slot = slot;

    public short Slot { get; }
}