using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PhoenixLib.Logging;
using WingsAPI.Packets.Enums.Shells;

namespace NosEmu.Plugins.BasicImplementations.Algorithms.Shells;

public interface IShellLevelEffectConfiguration
{
    IReadOnlyCollection<ShellPossibleCategory> GetEffects(byte shellType, byte rarity);
}

public class ShellLevelEffectConfiguration : IShellLevelEffectConfiguration
{
    private readonly ImmutableDictionary<(byte, byte), List<ShellPossibleCategory>> _shellEffects;

    public ShellLevelEffectConfiguration(IEnumerable<ShellLevelEffect> shellEffects)
    {
        var dict = new Dictionary<(byte, byte), List<ShellPossibleCategory>>();
        foreach (ShellLevelEffect shellEffect in shellEffects)
        {
            foreach (ShellRarityEffect shellRarityEffect in shellEffect.Rarities)
            {
                dict.Add(((byte)shellEffect.ShellType, shellRarityEffect.Rarity), shellRarityEffect.PossibleEffects);
            }
        }

        _shellEffects = dict.ToImmutableDictionary();
    }

    public IReadOnlyCollection<ShellPossibleCategory> GetEffects(byte shellType, byte rarity)
    {
        if (!_shellEffects.ContainsKey((shellType, rarity)))
        {
            Log.Debug($"[ERROR] A configuration for {shellType.ToString()} has not been found.");
            return null;
        }
        
        return _shellEffects[(shellType, rarity)];
    }
}

public class ShellLevelEffect
{
    public ShellType ShellType { get; set; }
    public List<ShellRarityEffect> Rarities { get; set; }
}

public class ShellRarityEffect
{
    public byte Rarity { get; set; }
    public List<ShellPossibleCategory> PossibleEffects { get; set; }
}

public class ShellPossibleCategory
{
    public ShellEffectCategory EffectCategory { get; set; }
    public bool IsRandom { get; set; }
}