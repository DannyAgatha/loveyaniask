using System;
using WingsEmu.Game.Raids;

namespace WingsEmu.Game.Networking.Broadcasting;

public class InRaidBroadcast : IBroadcastRule
{
    private readonly Guid _id;

    public InRaidBroadcast(RaidParty raid) => _id = raid.Id;

    public bool Match(IClientSession session) => session.PlayerEntity.Raid != null && session.PlayerEntity.Raid.Id == _id;
}