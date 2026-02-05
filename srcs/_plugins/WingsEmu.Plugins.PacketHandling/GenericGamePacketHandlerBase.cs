using System.Threading.Tasks;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;
using WingsEmu.Packets;

namespace WingsEmu.Plugins.PacketHandling;

public abstract class GenericGamePacketHandlerBase<T> : IGamePacketHandler where T : IPacket
{
    public async Task HandleAsync(IClientSession session, IPacket packet)
    {
        if (packet is T typedPacket && session.IsAuthenticated)
        {
            await HandlePacketAsync(session, typedPacket);
        }
    }

    public void Handle(IClientSession session, IPacket packet)
    {
        HandleAsync(session, packet).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected abstract Task HandlePacketAsync(IClientSession session, T packet);
}