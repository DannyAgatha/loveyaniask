using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public interface IPartnerSpecialistBasicConfig
{
    short GetAttackEffect(short morph);
}

public class PartnerSpecialistBasicConfig : IPartnerSpecialistBasicConfig
{
    private readonly ConcurrentDictionary<short, short> _effects = new();

    public PartnerSpecialistBasicConfig(IEnumerable<PartnerSpecialistBasicConfiguration> configurations)
    {
        foreach (PartnerSpecialistBasicConfiguration partner in configurations)
        {
            _effects.TryAdd(partner.MorphId, partner.Attack);
        }
    }

    public short GetAttackEffect(short morph) => (short)(_effects.TryGetValue(morph, out short attack) ? attack : 0);
}

public class PartnerSpecialistBasicConfiguration
{
    public short MorphId { get; set; }
    public short Attack { get; set; }
}