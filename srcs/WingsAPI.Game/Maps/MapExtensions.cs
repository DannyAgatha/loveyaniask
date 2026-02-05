using System;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Maps;

public static class MapExtensions
{
    public static bool CanWalkAround(this IMapInstance mapInstance, int x, int y)
    {
        for (int dX = -1; dX <= 1; dX++)
        {
            for (int dY = -1; dY <= 1; dY++)
            {
                if (mapInstance.IsBlockedZone(x + dX, y + dY))
                {
                    return false;
                }
            }
        }

        return true;
    }


    public static bool IsBlockedZone(this IMapInstance mapInstance, int x, int y)
    {
        try
        {
            if (mapInstance.Grid == null)
            {
                return false;
            }

            return !mapInstance.Grid.IsWalkable(x, y, mapInstance.Width, mapInstance.Height);
        }
        catch
        {
            return true;
        }
    }

    public static bool PvpZone(this IMapInstance mapInstance, int x, int y)
    {
        try
        {
            return mapInstance.Grid != null && mapInstance.Grid.IsPvpZoneOff(x, y, mapInstance.Width, mapInstance.Height);
        }
        catch
        {
            return false;
        }
    }

    public static bool MateDollZone(this IMapInstance mapInstance, int x, int y)
    {
        try
        {
            return mapInstance.Grid != null && mapInstance.Grid.IsMateDollZone(x, y, mapInstance.Width, mapInstance.Height);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsMonsterAggroDisabled(this IMapInstance mapInstance, int x, int y)
    {
        try
        {
            return mapInstance.Grid != null && mapInstance.Grid.IsMonsterAggroDisabled(x, y, mapInstance.Width, mapInstance.Height);
        }
        catch
        {
            return false;
        }
    }

    public static bool GetFreePosition(this IMapInstance mapInstance, IRandomGenerator randomGenerator, ref short firstX, ref short firstY, byte xpoint, byte ypoint)
    {
        short minX = (short)(-xpoint + firstX);
        short maxX = (short)(xpoint + firstX);

        short minY = (short)(-ypoint + firstY);
        short maxY = (short)(ypoint + firstY);

        short x = (short)randomGenerator.RandomNumber(minX, maxX + 1);
        short y = (short)randomGenerator.RandomNumber(minY, maxY + 1);

        if (mapInstance.IsPathBlocked(firstX, firstY, x, y))
        {
            return false;
        }

        firstX = x;
        firstY = y;
        return true;
    }

    private static bool IsPathBlocked(this IMapInstance mapInstance, int firstX, int firstY, int mapX, int mapY)
    {
        for (int i = 1; i <= Math.Abs(mapX - firstX); i++)
        {
            if (mapInstance.IsBlockedZone(firstX + Math.Sign(mapX - firstX) * i, firstY))
            {
                return true;
            }
        }

        for (int i = 1; i <= Math.Abs(mapY - firstY); i++)
        {
            if (mapInstance.IsBlockedZone(firstX, firstY + Math.Sign(mapY - firstY) * i))
            {
                return true;
            }
        }

        return false;
    }
}