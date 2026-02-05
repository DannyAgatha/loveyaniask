using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Extensions;

public static class MapItemExtension
{
    public static string GenerateIn(this MapItem mapItem) => $"in 9 {mapItem.ItemVNum} {mapItem.TransportId} {mapItem.PositionX} {mapItem.PositionY} {mapItem.Amount} {(mapItem.IsQuest ? 1 : 0)} 0 -1 0";

    public static string GenerateOut(this MapItem mapItem) => $"out 9 {mapItem.TransportId}";

    public static string GenerateSay(this MapItem mapItem) => $"say 9 {mapItem.TransportId} 2 Please, pick me up... quickly!";

    public static void BroadcastSayDrop(this MapItem mapItem) => mapItem.MapInstance.Broadcast(mapItem.GenerateSay());

    public static void BroadcastIn(this MapItem mapItem) => mapItem.MapInstance.Broadcast(mapItem.GenerateIn());

    public static void BroadcastOut(this MapItem mapItem) => mapItem.MapInstance.Broadcast(mapItem.GenerateOut());
}