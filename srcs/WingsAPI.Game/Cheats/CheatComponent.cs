namespace WingsEmu.Game.Cheats;

public class CheatComponent : ICheatComponent
{
    public bool HasOneHitKill { get; set; }
    public bool HasGodMode { get; set; }
    public bool IsInvisible { get; set; }
    public bool HasNoCooldown { get; set; }
    public bool HasNoTargetLimit { get; set; }
}