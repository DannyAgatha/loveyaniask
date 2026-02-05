namespace WingsEmu.Game.Raids;

public interface IRaidFactory
{
    RaidInstance CreateRaid(RaidParty raidType);
}