using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonRewardEvent : IAsyncEvent
{
    public DungeonInstanceWrapper DungeonInstanceWrapper { get; init; }
}

public class Act4DungeonWonEvent : IAsyncEvent
{
    public DungeonInstance DungeonInstance { get; init; }
    public IClientSession DungeonLeader { get; init; }
    public IEnumerable<IClientSession> Members { get; init; }
}