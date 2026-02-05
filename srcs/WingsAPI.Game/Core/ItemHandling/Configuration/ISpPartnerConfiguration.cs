using System.Collections.Generic;
using System.Collections.Immutable;

namespace WingsEmu.Game._ItemUsage.Configuration;

public interface ISpPartnerConfiguration
{
    SpPartnerInfo GetByMorph(short morphId);
}

public class SpPartnerConfiguration : ISpPartnerConfiguration
{
    private readonly ImmutableDictionary<int, SpPartnerInfo> _partnerInfo;

    public SpPartnerConfiguration(IEnumerable<SpPartnerInfo> partnerInfo)
    {
        _partnerInfo = partnerInfo.ToImmutableDictionary(s => s.MorphId);
    }

    public SpPartnerInfo GetByMorph(short morphId) => _partnerInfo.GetValueOrDefault(morphId);
}

public class SpPartnerInfo
{
    public int MorphId { get; set; }
    public string Name { get; set; }
    public int BuffId { get; set; }
}