using System.Collections.Generic;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface IBuffsToRemoveConfig
{
    short[] GetBuffsToRemove(BuffsToRemoveType type);
}

public class BuffsToRemoveConfig : IBuffsToRemoveConfig
{
    private readonly Dictionary<BuffsToRemoveType, short[]> _buffs = new();

    public BuffsToRemoveConfig(BuffsToRemoveConfiguration configuration)
    {
        _buffs[BuffsToRemoveType.ATTACKER] = configuration.AttackerBuffs;
        _buffs[BuffsToRemoveType.DEFENDER] = configuration.DefenderBuffs;
        _buffs[BuffsToRemoveType.PVP] = configuration.PvpBuffs;
    }

    public short[] GetBuffsToRemove(BuffsToRemoveType type) => _buffs.GetOrDefault(type);
}

public class BuffsToRemoveConfiguration
{
    public short[] AttackerBuffs { get; set; }
    public short[] DefenderBuffs { get; set; }
    public short[] PvpBuffs { get; set; }
}

public enum BuffsToRemoveType
{
    ATTACKER,
    DEFENDER,
    PVP
}