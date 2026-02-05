using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class AddObjMinilandEndLogicEventHandler : IAsyncEventProcessor<AddObjMinilandEndLogicEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMinilandManager _minilandManager;
    private readonly IRandomGenerator _randomGenerator;

    public AddObjMinilandEndLogicEventHandler(IMinilandManager minilandManager, IRandomGenerator randomGenerator, IGameLanguageService languageService)
    {
        _minilandManager = minilandManager;
        _randomGenerator = randomGenerator;
        _languageService = languageService;
    }

    public async Task HandleAsync(AddObjMinilandEndLogicEvent e, CancellationToken cancellation)
    {
        IMapInstance mapInstance = e.Miniland;

        if (mapInstance.MapDesignObjects.Exists(x => x.InventorySlot == e.MapObject.InventoryItem.Slot))
        {
            return;
        }

        WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration = _minilandManager.GetMinilandConfiguration(mapInstance);
        if (minilandConfiguration == default)
        {
            return;
        }

        bool isForced = false;

        if (e.MapObject.InventoryItem.ItemInstance.GameItem.ItemType == ItemType.House)
        {
            ForcedPlacing forcedPlacing = minilandConfiguration.ForcedPlacings.FirstOrDefault(x =>
                (int)x.SubType == e.MapObject.InventoryItem.ItemInstance.GameItem.ItemSubType);

            if (forcedPlacing != default)
            {
                e.MapObject.MapX = forcedPlacing.ForcedLocation.X;
                e.MapObject.MapY = forcedPlacing.ForcedLocation.Y;
            }

            isForced = true;
        }

        byte[] modifiedGrid = GetModifiedGrid(mapInstance, minilandConfiguration, e.MapObject, isForced);
        if (!IsZoneConstructable(modifiedGrid, e.MapObject, mapInstance.Width, mapInstance.Height))
        {
            IGameItem gameItem = e.MapObject.InventoryItem.ItemInstance.GameItem;
            e.Sender.SendDebugMessage($"[MINILAND] The zone is not constructable. Object Width: {gameItem.Width.ToString()} | Object Height: {gameItem.Height.ToString()}");
            return;
        }

        MoveMatesColliding(e, minilandConfiguration);

        mapInstance.MapDesignObjects.Add(e.MapObject);

        if (e.MapObject.InventoryItem.ItemInstance.GameItem.IsWarehouse)
        {
            e.Sender.PlayerEntity.WareHouseSize = e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint;
        }

        if (e.MapObject.InventoryItem.ItemInstance.GameItem.ItemType == ItemType.House)
        {
            _minilandManager.RelativeUpdateMinilandCapacity(e.Sender.PlayerEntity.Id, e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint);
            BroadcastCapacityUpdate(e);
        }

        mapInstance.Broadcast(e.MapObject.GenerateEffect(false));
        mapInstance.Broadcast(e.MapObject.GenerateMinilandObject(false));
    }

    private void BroadcastCapacityUpdate(AddObjMinilandEndLogicEvent e)
    {
        foreach (IClientSession session in e.Miniland.Sessions)
        {
            if (session.PlayerEntity.Id == e.Sender.PlayerEntity.Id)
            {
                continue;
            }

            session.SendMinilandPublicInformation(_minilandManager, _languageService);
        }
    }

    private void MoveMatesColliding(AddObjMinilandEndLogicEvent e, WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration)
    {
        (int x1, int y1, int x2, int y2) boundaries = GetMapObjectBoundaries(e.MapObject);

        RestrictedZone secureZoneForMate = minilandConfiguration.RestrictedZones.FirstOrDefault(x =>
            x.RestrictionTag == RestrictionType.OnlyMates);

        int baseX1 = default;
        int baseX2 = default;
        int baseY1 = default;
        int baseY2 = default;
        if (secureZoneForMate != default)
        {
            (baseX1, baseY1, baseX2, baseY2) = GetNormalizedBoundaries(
                secureZoneForMate.Corner1.X, secureZoneForMate.Corner1.Y, secureZoneForMate.Corner2.X, secureZoneForMate.Corner2.Y);
        }

        foreach (IMateEntity mateEntity in e.Miniland.GetAliveMates())
        {
            if (!IsInsideRectangle(boundaries, mateEntity.PositionX, mateEntity.PositionY))
            {
                continue;
            }

            if (secureZoneForMate != default && mateEntity.Owner.Id == e.Sender.PlayerEntity.Id)
            {
                mateEntity.ChangePosition(new Position((short)_randomGenerator.RandomNumber(baseX1, baseX2 + 1), (short)_randomGenerator.RandomNumber(baseY1, baseY2 + 1)));
                e.Sender.BroadcastMateTeleport(mateEntity);
            }
            else
            {
                mateEntity.TeleportToCharacter();
            }
        }
    }

    private static byte[] GetModifiedGrid(IMapInstance mapInstance, WingsEmu.Game.Configurations.Miniland.Miniland minilandConfiguration, MapDesignObject mapObject, bool forcedPlacing = false)
    {
        //TODO This should be reworked, it not needed to copy the map grid 
        byte[] mapGrid = forcedPlacing ? new byte[mapInstance.Width * mapInstance.Height] : mapInstance.Grid.ToArray();

        foreach (MapDesignObject mapDesignObject in mapInstance.MapDesignObjects)
        {
            (int x1, int y1, int x2, int y2) = GetMapObjectBoundaries(mapDesignObject);

            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    mapGrid[i + j * mapInstance.Width] = (byte)(mapGrid[i + j * mapInstance.Width] | 1);
                }
            }
        }

        foreach (IClientSession session in mapInstance.Sessions)
        {
            IPlayerEntity character = session.PlayerEntity;
            mapGrid[character.PositionX + character.PositionY * mapInstance.Width] =
                (byte)(mapGrid[character.PositionX + character.PositionY * mapInstance.Width] | 1);
        }

        foreach (RestrictedZone restrictedZone in minilandConfiguration.RestrictedZones)
        {
            bool blockZone = false;
            switch (restrictedZone.RestrictionTag)
            {
                case RestrictionType.OnlySoilObjects when mapObject.InventoryItem.ItemInstance.GameItem.ItemType != ItemType.Minigame:
                case RestrictionType.OnlyTerraceObjects when mapObject.InventoryItem.ItemInstance.GameItem.ItemType != ItemType.Terrace:
                case RestrictionType.OnlyGardenObjects when mapObject.InventoryItem.ItemInstance.GameItem.ItemType != ItemType.Garden:
                case RestrictionType.OnlyMates:
                case RestrictionType.Unconstructable:
                    blockZone = true;
                    break;
            }

            if (!blockZone)
            {
                continue;
            }

            (int x1, int y1, int x2, int y2) = GetNormalizedBoundaries(
                restrictedZone.Corner1.X, restrictedZone.Corner1.Y, restrictedZone.Corner2.X, restrictedZone.Corner2.Y);

            for (int i = x1; i <= x2; i++)
            {
                for (int j = y1; j <= y2; j++)
                {
                    mapGrid[i + j * mapInstance.Width] = (byte)(mapGrid[i + j * mapInstance.Width] | 1);
                }
            }
        }

        return mapGrid;
    }

    private static bool IsZoneConstructable(IReadOnlyList<byte> array, MapDesignObject mapObject, int width, int height)
    {
        (int x1, int y1, int x2, int y2) = GetMapObjectBoundaries(mapObject);

        for (int i = x1; i <= x2; i++)
        {
            for (int j = y1; j <= y2; j++)
            {
                if (!array.IsWalkable(i, j, width, height))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static (int x1, int y1, int x2, int y2) GetMapObjectBoundaries(MapDesignObject mapObject)
    {
        int x1 = mapObject.MapX;
        int y1 = mapObject.MapY;
        int x2 = mapObject.MapX + mapObject.InventoryItem.ItemInstance.GameItem.Width - 1;
        int y2 = mapObject.MapY + mapObject.InventoryItem.ItemInstance.GameItem.Height - 1;

        return GetNormalizedBoundaries(x1, y1, x2, y2);
    }

    private static (int x1, int y1, int x2, int y2) GetNormalizedBoundaries(int x1, int y1, int x2, int y2)
    {
        bool randBoolean1 = x1 < x2;
        int baseX1 = randBoolean1 ? x1 : x2;
        int baseX2 = randBoolean1 ? x2 : x1;

        bool randBoolean2 = y1 < y2;
        int baseY1 = randBoolean2 ? y1 : y2;
        int baseY2 = randBoolean2 ? y2 : y1;
        return (baseX1, baseY1, baseX2, baseY2);
    }

    private static bool IsInsideRectangle((int x1, int y1, int x2, int y2) boundaries, int mX, int mY)
    {
        (int x1, int y1, int x2, int y2) = boundaries;
        return IsInsideRectangle(mX, mY, x1, y1, x2, y2);
    }

    private static bool IsInsideRectangle(int mX, int mY, int x1, int y1, int x2, int y2) =>
        mX >= x1 && mX <= x2 &&
        mY >= y1 && mY <= y2;
}