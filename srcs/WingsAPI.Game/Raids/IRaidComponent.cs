using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Raids;

public interface IRaidComponent
{
    public RaidParty Raid { get; }
    public byte RaidDeaths { get; }
    
    public Position PositionBeforeFernonRaidEnter { get; }
    public void SetPositionBeforeFernonRaidEnter(Position position);

    public bool IsInRaidParty { get; }
    public bool HasRaidStarted { get; }
    public bool RaidTeamIsFull { get; }
    public bool IsRaidLeader(long characterId);

    public void SetRaidParty(RaidParty raidParty);
    public void AddRaidDeath();
    public void RemoveRaidDeath();
}