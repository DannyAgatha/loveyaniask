namespace WingsEmu.Game.Act4;

public interface IDungeonFactory
{
    DungeonInstance CreateDungeon(long familyId, DungeonType dungeonType);
}