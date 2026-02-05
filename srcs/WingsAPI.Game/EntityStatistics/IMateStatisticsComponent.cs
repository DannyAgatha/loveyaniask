using WingsEmu.Game.Mates;

namespace WingsEmu.Game.EntityStatistics;

public interface IMateStatisticsComponent
{
    public int MinDamage { get; }
    public int MaxDamage { get; }
    public int HitRate { get; }
    public int CriticalChance { get; }
    public int CriticalDamage { get; }
    public int MeleeDefense { get; }
    public int RangeDefense { get; }
    public int MagicDefense { get; }
    public int MeleeDodge { get; }
    public int RangeDodge { get; }
    public int FireResistance { get; }
    public int WaterResistance { get; }
    public int LightResistance { get; }
    public int ShadowResistance { get; }

    public void RefreshMateStatistics(IMateEntity mateEntity);
}