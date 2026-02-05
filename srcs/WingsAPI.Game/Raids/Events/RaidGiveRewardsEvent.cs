using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Raids.Events;

public class RaidGiveRewardsEvent : IAsyncEvent
{
    public RaidGiveRewardsEvent(RaidParty raidParty, IMonsterEntity mapBoss, RaidReward raidReward)
    {
        RaidParty = raidParty;
        MapBoss = mapBoss;
        RaidReward = raidReward;
    }

    public RaidParty RaidParty { get; }
    public IMonsterEntity MapBoss { get; }
    public RaidReward RaidReward { get; }
}