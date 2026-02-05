using PhoenixLib.Events;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event;

public class Act4MukrajuSpawnEvent : IAsyncEvent
{
    public Act4MukrajuSpawnEvent(FactionType factionType) => FactionType = factionType;

    public FactionType FactionType { get; }
}