using System.Collections.Generic;
using System.Collections.Immutable;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

public interface IRespawnDefaultConfiguration
{
    public RespawnDefault GetReturn(RespawnType type);
    public RespawnDefault GetReturnAct5(Act5RespawnType type);
}

public class RespawnDefaultConfiguration : IRespawnDefaultConfiguration
{
    private readonly ImmutableDictionary<RespawnType, RespawnDefault> _respawns;
    private readonly Dictionary<Act5RespawnType, RespawnDefault> _respawnsAct5 = new();

    public RespawnDefaultConfiguration(IEnumerable<RespawnDefault> returns)
    {
        _respawns = returns.ToImmutableDictionary(s => s.Name);
        foreach (RespawnDefault getReturn in returns)
        {
            Act5RespawnType? act5RespawnType = getReturn.Name switch
            {
                RespawnType.MORTAZ_DESERT_PORT => Act5RespawnType.MORTAZ_DESERT_PORT,
                RespawnType.AKAMUR_CAMP => Act5RespawnType.AKAMUR_CAMP,
                RespawnType.DESERT_EAGLE_CITY => Act5RespawnType.DESERT_EAGLE_CITY,
                _ => null
            };

            if (act5RespawnType is null)
            {
                continue;
            }

            _respawnsAct5[act5RespawnType.Value] = getReturn;
        }
    }

    public RespawnDefault GetReturn(RespawnType type) => _respawns.GetValueOrDefault(type);
    public RespawnDefault GetReturnAct5(Act5RespawnType type) => _respawnsAct5.GetValueOrDefault(type);
}

public class RespawnDefault
{
    public RespawnType Name { get; set; }
    public short MapId { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
    public byte Radius { get; set; }
}