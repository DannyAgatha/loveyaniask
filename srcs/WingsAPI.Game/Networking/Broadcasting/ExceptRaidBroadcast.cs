using System;

namespace WingsEmu.Game.Networking.Broadcasting;

public class ExceptRaidBroadcast : IBroadcastRule
{
    private readonly Guid _id;

    public ExceptRaidBroadcast(Guid raidId) => _id = raidId;

    public bool Match(IClientSession session) => session != null && (!session.PlayerEntity.IsInRaidParty || session.PlayerEntity.Raid != null && session.PlayerEntity.Raid.Id != _id);
}