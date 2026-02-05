using PhoenixLib.Events;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Raids.Events;

public class RaidTeleportMemberEvent : IAsyncEvent
{
    public RaidTeleportMemberEvent(IMapInstance mapInstance, Position sourcePosition, Position destinationPosition, byte range)
    {
        MapInstance = mapInstance;
        SourcePosition = sourcePosition;
        DestinationPosition = destinationPosition;
        Range = range;
    }

    public IMapInstance MapInstance { get; }
    public Position SourcePosition { get; }
    public Position DestinationPosition { get; }
    public byte Range { get; }
}