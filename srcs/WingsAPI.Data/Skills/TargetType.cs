namespace WingsEmu.DTOs.Skills;

public enum TargetType
{
    Target = 0, // e.g. basic attack
    Self = 1, // e.g. mage regeneration aura
    SelfOrTarget = 2, // e.g. holy heal
    NonTarget = 3 // e.g. scout tp
}