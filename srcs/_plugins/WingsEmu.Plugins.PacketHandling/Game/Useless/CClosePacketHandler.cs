using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class CClosePacketHandler : GenericGamePacketHandlerBase<CClosePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, CClosePacket packet)
    {
        if (!session.HasSelectedCharacter)
        {
            return;
        }
        
        bool isBankPreviouslyOpen = session.PlayerEntity.IsBankOpen;
        
        session.PlayerEntity.HasNosBazaarOpen = false;
        session.PlayerEntity.IsBankOpen = false;
        
        if (isBankPreviouslyOpen)
        {
            session.SendInfoi(Game18NConstString.ThankYouForUsingTheCuarryBank);
        }
    }
}