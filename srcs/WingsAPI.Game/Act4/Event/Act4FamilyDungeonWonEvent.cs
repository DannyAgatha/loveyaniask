using System.Collections.Generic;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Act4.Event;

public class Act4FamilyDungeonWonEvent : PlayerEvent
{
    public DungeonType DungeonType { get; init; }
    public long FamilyId { get; init; }
    public IEnumerable<IClientSession> Members { get; init; }
}