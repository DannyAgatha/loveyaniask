namespace WingsEmu.Game.Monster;

public class MonsterSubRace
{
    public enum Angels : byte
    {
        Angel = 0,
        Demon = 1,
        HalfAngel = 2
    }

    public enum Fixed : byte
    {
        FixedTrap = 0,
        EnergyBall = 1,
        CannonBall = 2,
        MiniLandStructure = 3,
        Unknown0 = 4,
        Unknown1 = 5,
        Unknown2 = 6,
        Unknown3 = 7,
        Unknown4 = 8
    }

    public enum Furry : byte
    {
        Kovolt = 0,
        Bushtail = 1,
        Catsy = 2
    }

    public enum HighLevel : byte
    {
        Plant = 0,
        Animal = 1,
        Monster = 2,
        Dragon = 3
    }

    public enum LowLevel : byte
    {
        Plant = 0,
        Animal = 1,
        Monster = 2,
        Dragon = 3
    }

    public enum Other : byte
    {
        Machine = 0,
        Doll = 1
    }

    public enum People : byte
    {
        Humanlike = 0,
        Elf = 1,
        Half = 2,
        Demon = 3,
        Orc = 4
    }

    public enum Spirits : byte
    {
        LowLevelSpirit = 0,
        HighLevelSpirit = 1,
        LowLevelGhost = 2,
        HighLevelGhost = 3
    }

    public enum Undead : byte
    {
        LowLevelUndead = 0,
        HighLevelUndead = 1,
        Vampire = 2
    }
}