using System.Collections.Generic;

namespace WingsEmu.Game.EntityStatistics;

public interface IPlayerStatisticsComponent
{
    IReadOnlyDictionary<PassiveType, int> Passives { get; }
    public int MinDamage { get; }
    public int MaxDamage { get; }
    public int HitRate { get; }
    public int CriticalChance { get; }
    public int CriticalDamage { get; }
    public int SecondMinDamage { get; }
    public int SecondMaxDamage { get; }
    public int SecondHitRate { get; }
    public int SecondCriticalChance { get; }
    public int SecondCriticalDamage { get; }
    public int MeleeDefense { get; }
    public int RangeDefense { get; }
    public int MagicDefense { get; }
    public int MeleeDodge { get; }
    public int RangeDodge { get; }
    public int FireResistance { get; }
    public int WaterResistance { get; }
    public int LightResistance { get; }
    public int ShadowResistance { get; }
    void RefreshPassives();
    public void RefreshPlayerStatistics();
}