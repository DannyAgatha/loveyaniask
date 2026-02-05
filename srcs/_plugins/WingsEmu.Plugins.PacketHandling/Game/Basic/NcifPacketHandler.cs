using System.Threading.Tasks;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class NcifPacketHandler : GenericGamePacketHandlerBase<NcifPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, NcifPacket ncifPacket)
    {
        if (!session.HasCurrentMapInstance && session.PlayerEntity.MapInstance != null)
        {
            return;
        }

        IBattleEntity entity = session.PlayerEntity.MapInstance.GetBattleEntity((VisualType)ncifPacket.Type, ncifPacket.TargetId);

        if (entity == null)
        {
            return;
        }

        if (entity.Id == session.PlayerEntity.Id)
        {
            return;
        }

        if (!entity.IsAlive())
        {
            return;
        }

        session.PlayerEntity.LastEntity = (entity.Type, entity.Id);
        session.SendStPacket(entity);
    }
}