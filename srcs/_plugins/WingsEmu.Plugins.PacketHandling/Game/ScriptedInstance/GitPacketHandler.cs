using System.Threading.Tasks;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class GitPacketHandler : GenericGamePacketHandlerBase<GitPacket>
{
    private readonly IDelayManager _delayManager;

    public GitPacketHandler(IDelayManager delayManager) => _delayManager = delayManager;

    protected override async Task HandlePacketAsync(IClientSession session, GitPacket packet)
    {
        MapItem mapItem = session.CurrentMapInstance.GetDrop(packet.ButtonId);
        if (mapItem == null)
        {
            return;
        }

        if (!await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.ButtonSwitch))
        {
            return;
        }

        switch (mapItem)
        {
            case ButtonMapItem buttonMapItem:

                if (buttonMapItem.CanBeMovedOnlyOnce.HasValue && buttonMapItem.CanBeMovedOnlyOnce.Value)
                {
                    return;
                }

                await session.EmitEventAsync(new RaidPlayerSwitchButtonEvent(buttonMapItem));
                break;
            case TimeSpaceMapItem timeSpaceMapItem:
                await session.EmitEventAsync(new TimeSpacePickUpItemEvent
                {
                    TimeSpaceMapItem = timeSpaceMapItem
                });
                break;
        }
    }
}