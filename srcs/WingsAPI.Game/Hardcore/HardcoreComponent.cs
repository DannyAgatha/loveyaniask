namespace WingsEmu.Game.Hardcore;

public interface IHardcoreComponent
{
    int TotalRaidDamage { get; set; }
    void ResetDamage();
}

public class HardcoreComponent : IHardcoreComponent
{
    public int TotalRaidDamage { get; set; }
    
    public void ResetDamage()
    {
        TotalRaidDamage = 0;
    }
}