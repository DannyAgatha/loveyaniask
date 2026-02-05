using System;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Portals;

public interface IPortalFactory
{
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos);
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, Guid destMapInstanceId, Position destPos);
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, int mapDestId, Position destPos);
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, int mapDestId, Position destPos, short? raidType, short? mapNameId, short? levelRequired, short? heroLevelRequired);
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, IMapInstance destination, Position destPos);
    IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, IMapInstance destination, Position destPos, PortalMinimapOrientation minimapOrientation);
}