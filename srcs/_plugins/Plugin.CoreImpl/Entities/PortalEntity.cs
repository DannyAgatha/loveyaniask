// NosEmu
// 


using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;

namespace Plugin.CoreImpl.Entities
{
    public class PortalEntity : IPortalEntity
    {
        private readonly Guid? _destinationMapId;
        private readonly int? _destinationMapMapId;
        private readonly IEventTriggerContainer _eventContainer;
        private IMapInstance? _destinationMap;


        public PortalEntity(IAsyncEventPipeline eventPipeline, PortalType portalType, IMapInstance mapInstance, Position position, short? raidType = null, short? mapNameId = null,
            Position? destPos = null,
            Guid? destinationMapId = null, IMapInstance? destinationMap = null, int? destinationMapMapId = null,
            PortalMinimapOrientation minimapOrientation = PortalMinimapOrientation.NORTH, short? levelRequired = null, short? heroLevelRequired = null)
        {
            Type = portalType;
            MapInstance = mapInstance;
            PositionX = position.X;
            PositionY = position.Y;
            MinimapOrientation = minimapOrientation;
            RaidType = raidType;
            MapNameId = mapNameId;
            LevelRequired = levelRequired;
            HeroLevelRequired = heroLevelRequired;
            
            if (destPos.HasValue)
            {
                DestinationX = destPos.Value.X;
                DestinationY = destPos.Value.Y;
            }

            _destinationMap = destinationMap;
            _destinationMapId = destinationMapId;
            _destinationMapMapId = destinationMapMapId;
            _eventContainer = new EventTriggerContainer(eventPipeline);
        }

        private static IMapManager _mapManager => StaticMapManager.Instance;

        public int Id { get; set; }
        public PortalType Type { get; set; }
        public bool IsDisabled { get; set; }
        public PortalMinimapOrientation MinimapOrientation { get; set; }
        public IMapInstance MapInstance { get; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }

        public short? RaidType { get; set; }
        public short? MapNameId { get; set; }
        public short? DestinationX { get; set; }
        public short? DestinationY { get; set; }
        public short? LevelRequired { get; set; }
        public short? HeroLevelRequired { get; set; }

        public IMapInstance? DestinationMapInstance
        {
            get
            {
                if (_destinationMap != null)
                {
                    return _destinationMap;
                }

                if (_destinationMapId.HasValue)
                {
                    return _destinationMap = _mapManager.GetMapInstance(_destinationMapId.Value);
                }

                if (_destinationMapMapId.HasValue)
                {
                    return _destinationMap = _mapManager.GetBaseMapInstanceByMapId(_destinationMapMapId.Value);
                }

                return null;
            }
        }


        public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false)
        {
            _eventContainer.AddEvent(key, notification, removedOnTrigger);
        }

        public async Task TriggerEvents(string key)
        {
            await _eventContainer.TriggerEvents(key);
        }
    }
}