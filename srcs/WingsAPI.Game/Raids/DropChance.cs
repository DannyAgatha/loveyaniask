namespace WingsEmu.Game.Raids;

public class DropChance
{
    public DropChance(int chance, int itemVnum, int amount)
    {
        Chance = chance;
        ItemVnum = itemVnum;
        Amount = amount;
    }

    public int Chance { get; }
    public int ItemVnum { get; }
    public int Amount { get; }
}