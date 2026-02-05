using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class AddObjMinilandEvent : PlayerEvent
{
    public AddObjMinilandEvent(short slot, short x, short y)
    {
        Slot = slot;
        X = x;
        Y = y;
    }

    public short Slot { get; }

    public short X { get; }

    public short Y { get; }
}