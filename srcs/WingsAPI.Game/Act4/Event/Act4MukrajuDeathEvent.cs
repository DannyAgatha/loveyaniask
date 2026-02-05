using PhoenixLib.Events;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event;

public class Act4MukrajuDeathEvent : IAsyncEvent
{
    public Act4MukrajuDeathEvent(FactionType factionType) => FactionType = factionType;

    public FactionType FactionType { get; }
}