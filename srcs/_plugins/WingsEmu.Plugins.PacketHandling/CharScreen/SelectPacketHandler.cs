// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Health;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class SelectPacketHandler : GenericCharScreenPacketHandlerBase<SelectPacket>
{
    private readonly IMaintenanceManager _maintenanceManager;

    public SelectPacketHandler(IMaintenanceManager maintenanceManager) => _maintenanceManager = maintenanceManager;

    protected override async Task HandlePacketAsync(IClientSession session, SelectPacket packet)
    {
        if (_maintenanceManager.IsMaintenanceActive && session.Account.Authority < AuthorityType.GM)
        {
            session.ForceDisconnect();
            return;
        }

        await session.EmitEventAsync(new CharacterPreLoadEvent(packet.Slot));
    }
}