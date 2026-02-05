using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FhistCtsPacketHandler : GenericGamePacketHandlerBase<FhistCtsPacket>
{
    private readonly IGameLanguageService _gameLanguage;

    public FhistCtsPacketHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    protected override async Task HandlePacketAsync(IClientSession session, FhistCtsPacket packet)
    {
        if (!session.PlayerEntity.IsInFamily())
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
            return;
        }

        session.SendFamilyLogsToMember(session.PlayerEntity.Family);
    }
}