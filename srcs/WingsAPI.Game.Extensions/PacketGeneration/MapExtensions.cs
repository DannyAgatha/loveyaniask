using System;
using System.Linq;
using PhoenixLib.Scheduler;
using WingsEmu.Game;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class MapExtensions
    {
        public static string GenerateDrop(this MapItem mapItem) =>
            $"drop {mapItem.ItemVNum} {mapItem.TransportId} {mapItem.PositionX} {mapItem.PositionY} {mapItem.Amount} {(mapItem.IsQuest ? 1 : 0)} {(mapItem is MonsterMapItem monsterMapItem ? monsterMapItem.OwnerId ?? -1 : -1)}";

        public static string GenerateMapDesignObjects(this IMapInstance map) => map.MapDesignObjects.Aggregate("mltobj", (current, mp) => current
            + $" {mp.InventoryItem.ItemInstance.ItemVNum.ToString()}.{mp.InventoryItem.Slot.ToString()}.{mp.MapX.ToString()}.{mp.MapY.ToString()}");

        public static void BroadcastDrop(this MapItem mapItem) => mapItem.MapInstance.Broadcast(mapItem.GenerateDrop());

        public static void AddPortalToMap(this IMapInstance mapInstance, IPortalEntity portal, IScheduler scheduler = null, int timeInSeconds = 0, bool isTemporary = false, bool sendGpPacket = true)
        {
            mapInstance.Portals.Add(portal);
            
            if (sendGpPacket)
            {
                mapInstance.Broadcast(portal.GenerateGp());
            }
            
            if (isTemporary)
            {
                scheduler?.Schedule(TimeSpan.FromSeconds(timeInSeconds), o => { mapInstance.DeletePortal(portal); });
            }
        }

        public static void DeletePortal(this IMapInstance mapInstance, IPortalEntity portal)
        {
            mapInstance.Portals.Remove(portal);
            mapInstance.MapClear();
        }

        public static IPortalEntity GetClosestPortal(this IMapInstance mapInstance, short posX, short posY, PortalType portalType = PortalType.TSNormal)
        {
            return mapInstance.Portals.Where(x => x.Type == portalType).OrderBy(x => Math.Abs(x.PositionX - posX)).ThenBy(x => Math.Abs(x.PositionY - posY)).FirstOrDefault();
        }

        public static void MapClear(this IMapInstance mapInstance, bool onlyItemsAndPortals = false)
        {
            mapInstance.Broadcast(mapInstance.GenerateMapClear());
            mapInstance.Broadcast(mapInstance.GetEntitiesOnMapPackets(onlyItemsAndPortals));
        }

        public static void BroadcastTimeSpacePartnerInfo(this IMapInstance mapInstance)
        {
            if (mapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
            {
                return;
            }

            foreach (INpcEntity npc in mapInstance.GetAliveNpcs(x => x.CharacterPartnerId.HasValue))
            {
                if (!npc.CharacterPartnerId.HasValue)
                {
                    continue;
                }

                IPlayerEntity player = mapInstance.GetCharacterById(npc.CharacterPartnerId.Value);
                if (player == null)
                {
                    continue;
                }

                player.Session.SendMateControl(npc);
                player.Session.SendCondMate(npc);
            }
        }
    }
}