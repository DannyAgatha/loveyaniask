using System;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Portals;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Packets.Enums;

namespace Plugin.CoreImpl.Entities
{
    public class PortalFactory : IPortalFactory
    {
        private readonly IAsyncEventPipeline _eventPipeline;

        public PortalFactory(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos) => new PortalEntity(_eventPipeline, portalType, source, sourcePos);

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, Guid destMapInstanceId, Position destPos) =>
            new PortalEntity(_eventPipeline, portalType, source, sourcePos, destPos: destPos, destinationMapId: destMapInstanceId);

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, int mapDestId, Position destPos) =>
            new PortalEntity(_eventPipeline, portalType, source, sourcePos, destinationMapMapId: mapDestId, destPos: destPos);

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, int mapDestId, Position destPos, short? raidType, short? mapNameId) =>
            new PortalEntity(_eventPipeline, portalType, source, sourcePos, raidType, mapNameId, destinationMapMapId: mapDestId, destPos: destPos);

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, IMapInstance destination, Position destPos) =>
            CreatePortal(portalType, source, sourcePos, destination, destPos, PortalMinimapOrientation.NORTH);

        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, IMapInstance destination, Position destPos, PortalMinimapOrientation minimapOrientation) =>
            new PortalEntity(_eventPipeline, portalType, source, sourcePos, destPos: destPos, destinationMap: destination, minimapOrientation: minimapOrientation);
        
        public IPortalEntity CreatePortal(PortalType portalType, IMapInstance source, Position sourcePos, int mapDestId, Position destPos, short? raidType, short? mapNameId, short? levelRequired, short? heroLevelRequired) =>
            new PortalEntity(_eventPipeline, portalType, source, sourcePos, raidType, mapNameId, destinationMapMapId: mapDestId, destPos: destPos, levelRequired: levelRequired, heroLevelRequired: heroLevelRequired);
    }
}