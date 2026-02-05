using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CSkillPacketHandler : GenericGamePacketHandlerBase<CSkillPacket>
{
    private readonly IGameLanguageService _gameLanguage;

    public CSkillPacketHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    protected override async Task HandlePacketAsync(IClientSession session, CSkillPacket packet)
    {
        IMapInstance mapInstanceType = session.CurrentMapInstance;

        if (!mapInstanceType.HasMapFlag(MapFlags.IS_BASE_MAP) && !mapInstanceType.HasMapFlag(MapFlags.ACT_4))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new BazaarOpenUiEvent(true));
    }
}