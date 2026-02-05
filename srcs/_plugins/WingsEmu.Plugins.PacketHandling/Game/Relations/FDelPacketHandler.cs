using System.Threading.Tasks;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class FDelPacketHandler : GenericGamePacketHandlerBase<FDelPacket>
{
    private readonly IGameLanguageService _language;

    public FDelPacketHandler(IGameLanguageService language) => _language = language;

    protected override async Task HandlePacketAsync(IClientSession session, FDelPacket packet)
    {
        if (session.PlayerEntity.IsMarried(packet.CharacterId))
        {
            return;
        }

        if (!session.PlayerEntity.IsFriend(packet.CharacterId))
        {
            return;
        }
        
        if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
        {
            return;
        }

        await session.RemoveRelationAsync(packet.CharacterId, CharacterRelationType.Friend);
        session.SendInfo(_language.GetLanguage(GameDialogKey.FRIEND_INFO_DELETED, session.UserLanguage));
    }
}