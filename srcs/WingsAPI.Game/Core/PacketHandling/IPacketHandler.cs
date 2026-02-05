// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets;

namespace WingsEmu.Game._packetHandling;

public interface IPacketHandler
{
    Task HandleAsync(IClientSession session, IPacket packet);
}