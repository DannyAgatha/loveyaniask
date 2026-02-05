using System.Collections.Generic;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorStartEvent : PlayerEvent
{
    public List<IClientSession> RedTeam { get; init; }
    public List<IClientSession> BlueTeam { get; init; }
}