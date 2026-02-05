using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Raids;

public class RaidComponent : IRaidComponent
{
    public RaidParty Raid { get; private set; }
    public byte RaidDeaths { get; private set; }
    
    public Position PositionBeforeFernonRaidEnter { get; private set; }

    public bool IsRaidLeader(long characterId)
    {
        if (!IsInRaidParty)
        {
            return false;
        }

        if (Raid?.Members == null || Raid.Members.Count < 1)
        {
            return false;
        }

        IClientSession leader = Raid.Members[0];
        return leader?.PlayerEntity.Id == characterId;
    }

    public bool RaidTeamIsFull => Raid != null && Raid.Members.Count >= Raid.MaximumMembers;
    
    public void SetPositionBeforeFernonRaidEnter(Position position)
    {
        PositionBeforeFernonRaidEnter = position;
    }

    public bool IsInRaidParty => Raid != null;

    public bool HasRaidStarted => Raid is { Started: true };

    public void SetRaidParty(RaidParty raidParty)
    {
        Raid = raidParty;
        RaidDeaths = 0;
    }

    public void AddRaidDeath()
    {
        RaidDeaths++;
    }

    public void RemoveRaidDeath()
    {
        if (RaidDeaths < 1)
        {
            RaidDeaths = 0;
            return;
        }

        RaidDeaths--;
    }
}