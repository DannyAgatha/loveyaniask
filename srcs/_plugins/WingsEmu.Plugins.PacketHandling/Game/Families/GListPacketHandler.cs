// NosEmu
// 


using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class GListPacketHandler : GenericGamePacketHandlerBase<GListPacket>
{
    private readonly FamilyConfiguration _familyConfiguration;
    private readonly ISessionManager _sessionService;

    public GListPacketHandler(ISessionManager sessionService, FamilyConfiguration familyConfiguration)
    {
        _sessionService = sessionService;
        _familyConfiguration = familyConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, GListPacket packet)
    {
        if (!session.PlayerEntity.IsInFamily())
        {
            return;
        }

        IFamily family = session.PlayerEntity.Family;

        switch (packet.Type)
        {
            case GListPacketType.RefreshFamilyInfo:
                session.RefreshFamilyInfo(family, _familyConfiguration);
                break;
            case GListPacketType.RefreshFamilyMembers:
                session.RefreshFamilyMembers(_sessionService, family);
                session.RefreshFamilyMembersExp(family);
                session.RefreshFamilyMembersMessages(family);
                break;
            default:
                return;
        }
    }
}