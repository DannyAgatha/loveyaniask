namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceRewardItem
{
    public TimeSpaceRewardType Type { get; set; }
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
    public sbyte Rarity { get; set; }
}

public enum TimeSpaceRewardType
{
    DRAW,
    BONUS,
    SPECIAL
}