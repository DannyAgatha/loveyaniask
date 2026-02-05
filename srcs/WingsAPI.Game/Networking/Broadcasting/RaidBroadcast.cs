using System;

namespace WingsEmu.Game.Networking.Broadcasting;

public class RaidBroadcast : IBroadcastRule
{
    private readonly Guid _raidId;

    public RaidBroadcast(Guid raidId) => _raidId = raidId;

    public bool Match(IClientSession session) => session != null && session.PlayerEntity.Raid != null && session.PlayerEntity.Raid.Id == _raidId;
}