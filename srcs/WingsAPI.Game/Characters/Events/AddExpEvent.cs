using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class AddExpEvent : PlayerEvent
{
    public AddExpEvent(long exp, LevelType levelType)
    {
        Exp = exp;
        LevelType = levelType;
    }

    public long Exp { get; }
    public LevelType LevelType { get; }
}