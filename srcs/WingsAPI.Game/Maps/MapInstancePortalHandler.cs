// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Maps;

public static class MapInstancePortalHandler
{
    public static List<IPortalEntity> GenerateMinilandEntryPortals(this IMapInstance mapInstance, IMapInstance miniland, IPortalFactory portalFactory)
    {
        var list = new List<IPortalEntity>();
        
        if (mapInstance.MapInstanceType is MapInstanceType.PrivateInstance)
        {
            return list;
        }
        
        switch (mapInstance.MapId)
        {
            case 1:
                list.Add(portalFactory.CreatePortal(PortalType.Miniland, mapInstance, new Position(48, 132), miniland, new Position(5, 8)));
                break;

            case 145:
                list.Add(portalFactory.CreatePortal(PortalType.Miniland, mapInstance, new Position(9, 171), miniland, new Position(5, 8)));
                break;
        }

        return list;
    }
}