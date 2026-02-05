namespace WingsEmu.Game.Bazaar.Configuration;

public class BazaarConfiguration
{
    public int MaximumListedItems { get; set; } = 30;

    public int MaximumListedItemsMedal { get; set; } = 90;

    public int DelayClientBetweenRequestsInSecs { get; set; } = 3;

    public int DelayServerBetweenRequestsInSecs { get; set; } = 1;

    public int ItemsPerIndex { get; set; } = 30;
}