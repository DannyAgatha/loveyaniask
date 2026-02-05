namespace WingsEmu.Game.TimeSpaces;

public interface ITimeSpaceFactory
{
    TimeSpaceInstance Create(TimeSpaceParty timeSpaceParty);
}