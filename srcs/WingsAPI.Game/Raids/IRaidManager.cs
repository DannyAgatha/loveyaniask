using System;
using System.Collections.Generic;

namespace WingsEmu.Game.Raids;

public interface IRaidManager
{
    IReadOnlyCollection<RaidParty> Raids { get; }
    void AddRaid(RaidParty raidParty);
    void RemoveRaid(RaidParty raidParty);

    void AddRaidPartyByMapInstanceId(Guid id, RaidParty raidParty);
    void RemoveRaidPartyByMapInstanceId(Guid id);
    RaidParty GetRaidPartyByMapInstanceId(Guid mapInstanceId);

    #region RAID_LIST

    IReadOnlyCollection<RaidParty> RaidPublishList { get; }
    bool ContainsRaidInRaidPublishList(RaidParty raid);
    void RegisterRaidInRaidPublishList(RaidParty raidParty);
    void UnregisterRaidFromRaidPublishList(RaidParty raid);

    #endregion
}