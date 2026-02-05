namespace WingsEmu.Game.Helpers.Damages.Calculation;

public class CalculationPhysicalDamage
{
    public int FinalAttack { get; set; }

    public double DamageMultiplier { get; set; }

    public double SoftDamageMultiplier { get; set; }

    public double FinalAttackMultiplier { get; set; }

    public double MapBasedDamageMultiplier { get; set; }

    public double PhysicalDamageMultiplier { get; set; }

    public double MagicDefenseAttackMultiplier { get; set; }

    public double PveDamageMultiplier { get; set; }

    public double PvpDamageMultiplier { get; set; }

    public double SkillBasedDamageMultiplier { get; set; }

    public double ElementBasedDamageMultiplier { get; set; }

    public double ElementalDamageMultiplier { get; set; }

    public double ElementalSoftDamageMultiplier { get; set; }

    public double ElementBasedDefenseMultiplier { get; set; }

    public double PhysicalDamageMultiplierByType { get; set; }
}