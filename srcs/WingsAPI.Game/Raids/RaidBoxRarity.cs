namespace WingsEmu.Game.Raids;

public class RaidBoxRarity
{
    public RaidBoxRarity(byte rarity, int chance)
    {
        Rarity = rarity;
        Chance = chance;
    }

    public byte Rarity { get; }
    public int Chance { get; }
}