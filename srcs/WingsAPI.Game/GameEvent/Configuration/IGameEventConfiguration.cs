using WingsEmu.Game.Maps;

namespace WingsEmu.Game.GameEvent.Configuration;

public interface IGameEventConfiguration
{
    public GameEventType GameEventType { get; }

    public short MapId { get; }

    public MapInstanceType MapInstanceType { get; }
}