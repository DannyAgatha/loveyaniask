namespace WingsEmu.Game.Helpers.Damages.Calculation;

public class CalculationResult
{
    public CalculationResult(int damage, bool isCritical, bool isSoftDamage)
    {
        Damage = damage;
        IsCritical = isCritical;
        IsSoftDamage = isSoftDamage;
    }

    public int Damage { get; }
    public bool IsCritical { get; }
    public bool IsSoftDamage { get; }
}