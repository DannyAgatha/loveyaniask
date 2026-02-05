using System.Collections.Generic;
using System.Collections.Immutable;
using WingsAPI.Packets.Enums.Shells;

namespace NosEmu.Plugins.BasicImplementations.Algorithms.Shells;

public interface IShellOptionTypeConfiguration
{
    int[] GetByTypeAndEffect(byte optionType, byte effect);
}

public class ShellOptionTypeConfiguration : IShellOptionTypeConfiguration
{
    private readonly ImmutableDictionary<byte, ShellOptionValues> _optionValues;

    public ShellOptionTypeConfiguration(IEnumerable<ShellOptionValues> optionValues)
    {
        _optionValues = optionValues.ToImmutableDictionary(s => (byte)s.Id);
    }

    public int[] GetByTypeAndEffect(byte optionType, byte effect)
    {
        switch (effect)
        {
            case (byte)ShellEffectCategory.CNormalWeapon:
            case (byte)ShellEffectCategory.CBonusWeapon:
            case (byte)ShellEffectCategory.CPVPWeapon:
            case (byte)ShellEffectCategory.CNormalArmor:
            case (byte)ShellEffectCategory.CBonusArmor:
            case (byte)ShellEffectCategory.CPVPArmor:
                return _optionValues.GetValueOrDefault(optionType)?.CRangeValues;
            case (byte)ShellEffectCategory.BNormalWeapon:
            case (byte)ShellEffectCategory.BBonusWeapon:
            case (byte)ShellEffectCategory.BPVPWeapon:
            case (byte)ShellEffectCategory.BNormalArmor:
            case (byte)ShellEffectCategory.BBonusArmor:
            case (byte)ShellEffectCategory.BPVPArmor:
                return _optionValues.GetValueOrDefault(optionType)?.BRangeValues;
            case (byte)ShellEffectCategory.ANormalWeapon:
            case (byte)ShellEffectCategory.ABonusWeapon:
            case (byte)ShellEffectCategory.APVPWeapon:
            case (byte)ShellEffectCategory.ANormalArmor:
            case (byte)ShellEffectCategory.ABonusArmor:
            case (byte)ShellEffectCategory.APVPArmor:
                return _optionValues.GetValueOrDefault(optionType)?.ARangeValues;
            case (byte)ShellEffectCategory.SNormalWeapon:
            case (byte)ShellEffectCategory.SBonusWeapon:
            case (byte)ShellEffectCategory.SPVPWeapon:
            case (byte)ShellEffectCategory.SNormalArmor:
            case (byte)ShellEffectCategory.SBonusArmor:
            case (byte)ShellEffectCategory.SPVPArmor:
                return _optionValues.GetValueOrDefault(optionType)?.SRangeValues;
        }

        return null;
    }
}

public class ShellOptionValues
{
    public ShellEffectType Id { get; set; }
    public int[] CRangeValues { get; set; }
    public int[] BRangeValues { get; set; }
    public int[] ARangeValues { get; set; }
    public int[] SRangeValues { get; set; }
}