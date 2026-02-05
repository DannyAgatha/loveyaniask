using WingsEmu.Game.Maps;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game;

public interface IPortalEntity : IEventTriggerContainer
{
    int Id { get; set; }
    PortalType Type { get; set; }
    bool IsDisabled { get; set; }
    IMapInstance MapInstance { get; }
    short PositionX { get; set; }
    short PositionY { get; set; }
    PortalMinimapOrientation MinimapOrientation { get; set; }
    short? RaidType { get; set; }
    short? MapNameId { get; set; }
    short? DestinationX { get; set; }
    short? DestinationY { get; set; }
    short? LevelRequired { get; set; }
    short? HeroLevelRequired { get; set; }
    IMapInstance? DestinationMapInstance { get; }
}