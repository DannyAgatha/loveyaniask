namespace WingsEmu.Game.Helpers.Damages.Calculation;

public class CalculationBasicStatistics
{
    #region Attacker

    public int AttackerMorale { get; set; }
    public int AttackerAttackUpgrade { get; set; }
    public int AttackerHitRate { get; set; }
    public int AttackerCriticalChance { get; set; }
    public int AttackerCriticalDamage { get; set; }
    public int AttackerElementRate { get; set; }
    public int AttackerMagicalDefense { get; set; }

    #endregion

    #region Defender

    public int DefenderMorale { get; set; }
    public int DefenderDefenseUpgrade { get; set; }
    public int DefenderDefense { get; set; }
    public int DefenderDodge { get; set; }
    public int DefenderResistance { get; set; }
    
    public int DefenderDefenseArmour { get; set; }

    #endregion
}