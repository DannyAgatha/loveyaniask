using System.Collections.Generic;

namespace WingsEmu.Game.Networking.Broadcasting;

public class OnlyPlayersBroadcast : IBroadcastRule
{
    private readonly HashSet<long> _players;

    public OnlyPlayersBroadcast(params long[] ids) => _players = new HashSet<long>(ids);

    public bool Match(IClientSession session) => session.PlayerEntity != null && _players.Contains(session.PlayerEntity.Id);
}