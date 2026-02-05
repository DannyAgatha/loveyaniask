// NosEmu
// 


namespace WingsEmu.Game.Cheats;

public interface ICheatComponent
{
    public bool HasGodMode { get; set; }
    public bool IsInvisible { get; set; }
    public bool HasNoCooldown { get; set; }
    public bool HasNoTargetLimit { get; set; }
}