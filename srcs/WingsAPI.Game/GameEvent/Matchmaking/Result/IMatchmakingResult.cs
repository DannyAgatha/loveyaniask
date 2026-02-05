using System;
using System.Collections.Generic;
using WingsEmu.Game._i18n;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.GameEvent.Matchmaking.Result;

public interface IMatchmakingResult
{
    public List<Tuple<IGameEventConfiguration, List<IClientSession>>> Sessions { get; }

    public Dictionary<Game18NConstString, List<IClientSession>> RefusedSessions { get; }
}