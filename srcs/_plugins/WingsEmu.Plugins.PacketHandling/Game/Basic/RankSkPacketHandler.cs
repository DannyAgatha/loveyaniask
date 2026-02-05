using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class RankSkPacketHandler : GenericGamePacketHandlerBase<RankSkPacket>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _eventPipeline;

    public RankSkPacketHandler(IBuffFactory buffFactory, IAsyncEventPipeline eventPipeline)
    {
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }

    protected override async Task HandlePacketAsync(IClientSession session, RankSkPacket packet)
    {
        /* TODO
        if (session.PlayerEntity.IsReputHero() <= 3)
        {
            return;
        }*/
    }
}