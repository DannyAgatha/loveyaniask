using System.Threading.Tasks;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class EscapePacketHandler : GenericGamePacketHandlerBase<EscapePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, EscapePacket packet)
    {
        switch (session.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.RainbowBattle:
                await session.EmitEventAsync(new RainbowBattleLeaveEvent
                {
                    CheckIfFinished = true
                });
                break;
            case MapInstanceType.RaidInstance:
                await session.EmitEventAsync(new RaidPartyLeaveEvent(false));
                break;
            case MapInstanceType.TimeSpaceInstance:
                await session.EmitEventAsync(new TimeSpaceLeavePartyEvent
                {
                    CheckFinished = true
                });
                break;
        }
    }
}