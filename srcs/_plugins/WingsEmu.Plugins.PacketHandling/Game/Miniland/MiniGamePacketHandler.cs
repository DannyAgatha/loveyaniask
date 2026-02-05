using System;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class MiniGamePacketHandler : GenericGamePacketHandlerBase<MinigamePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, MinigamePacket packet)
    {
        if (packet == null
            || session.CurrentMapInstance.MapInstanceType != MapInstanceType.Miniland)
        {
            return;
        }

        MapDesignObject mapObject = session.CurrentMapInstance?.MapDesignObjects.FirstOrDefault(s => s.InventoryItem.Slot == packet.Id);
        if (mapObject == null
            || mapObject.InventoryItem.ItemInstance.GameItem.ItemType != ItemType.Minigame)
        {
            return;
        }

        RewardLevel rewardLevel;
        switch (packet.Type)
        {
            case 1:
                await session.EmitEventAsync(new MinigamePlayEvent(mapObject, false));
                break;
            case 2:
                await session.EmitEventAsync(new MinigameStopEvent(mapObject));
                break;
            case 3:
                if (packet.Point == null || packet.Point2 == null)
                {
                    return;
                }

                await session.EmitEventAsync(new MinigameScoreEvent(mapObject, packet.Point.Value, packet.Point2.Value));
                break;
            case 4:
                if (packet.Point == null)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(RewardLevel), packet.Point))
                {
                    throw new ArgumentOutOfRangeException("", "The RewardLevel (Minigame) that was reclaimed doesn't exist in the Enum.");
                }

                rewardLevel = (RewardLevel)packet.Point;

                bool coupon = packet.Point2 != null;
                await session.EmitEventAsync(new MinigameRewardEvent(rewardLevel, mapObject, coupon));
                break;
            case 5:
                await session.EmitEventAsync(new MinigameDurabilityInfoEvent(mapObject));
                break;
            case 6:
                if (packet.Point == null)
                {
                    return;
                }

                await session.EmitEventAsync(new MinigameRepairDurabilityEvent(mapObject, Math.Abs(packet.Point.Value)));
                break;
            case 7:
                await session.EmitEventAsync(new MinigameGetYieldInfoEvent(mapObject));
                break;
            case 8:
                if (packet.Point == null)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(RewardLevel), packet.Point))
                {
                    throw new ArgumentOutOfRangeException("", "The RewardLevel (Minigame) that was reclaimed doesn't exist in the Enum.");
                }

                session.EmitEvent(new MinigameGetYieldRewardEvent(mapObject, (RewardLevel)packet.Point));
                break;
            case 9:
                await session.EmitEventAsync(new MinigameDurabilityCouponEvent(mapObject));
                break;
            case 10:
                await session.EmitEventAsync(new MinigamePlayEvent(mapObject, true));
                break;
        }
    }
}