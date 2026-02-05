// NosEmu
// 


using WingsAPI.Data.Families;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyAddLogEvent : PlayerEvent
{
    public FamilyAddLogEvent(FamilyLogDto log) => Log = log;

    public FamilyLogDto Log { get; }
}