using System;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.GameEvent;

public interface IGameEventInstance
{
    public IGameEventConfiguration Configuration { get; }

    public DateTime DestroyDate { get; }

    public IMapInstance MapInstance { get; }

    public GameEventType GameEventType { get; }
}