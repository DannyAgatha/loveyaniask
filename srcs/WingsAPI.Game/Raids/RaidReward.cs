namespace WingsEmu.Game.Raids;

public class RaidReward
{
    public RaidReward(RaidBox raidBox, bool defaultReputation, int? fixedReputation)
    {
        RaidBox = raidBox;
        DefaultReputation = defaultReputation;
        FixedReputation = fixedReputation;
    }

    public RaidBox RaidBox { get; }
    public bool DefaultReputation { get; }
    public int? FixedReputation { get; }
}