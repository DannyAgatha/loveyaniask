using WingsEmu.Game.Maps;

namespace WingsEmu.Game;

public static class PortalPacketExtensions
{
    public static string GenerateGp(this IPortalEntity portal)
    {
        if (portal.DestinationMapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            return
                $"gp {portal.PositionX} {portal.PositionY} {portal.MapNameId ?? portal.DestinationMapInstance.MapId} {(sbyte)portal.Type} {(byte)portal.MinimapOrientation} {(portal.IsDisabled ? 1 : 0)}";
        }

        return $"gp {portal.PositionX} {portal.PositionY} {portal.MapNameId ?? portal.DestinationMapInstance?.MapId ?? 0} {(sbyte)portal.Type} {portal.Id} {(portal.IsDisabled ? 1 : 0)}";
    }
}