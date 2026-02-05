using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Maps.Event;

public class JoinMapEndEvent : PlayerEvent
{
    public JoinMapEndEvent(IMapInstance joinedMapInstance) => JoinedMapInstance = joinedMapInstance;

    public IMapInstance JoinedMapInstance { get; }
}