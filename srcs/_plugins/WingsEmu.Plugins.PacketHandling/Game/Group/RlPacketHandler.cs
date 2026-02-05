using System;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Group;

public class RlPacketHandler : GenericGamePacketHandlerBase<RlPacket>
{
    private readonly IGameLanguageService _language;

    public RlPacketHandler(IGameLanguageService language) => _language = language;

    protected override async Task HandlePacketAsync(IClientSession session, RlPacket rlPacket)
    {
        if (session.PlayerEntity.Level < 20)
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_TOO_LOW_LVL, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        if (!Enum.TryParse(rlPacket.Type.ToString(), out RlClientPacketType type))
        {
            return;
        }

        switch (type)
        {
            case RlClientPacketType.REGISTER:
                await session.EmitEventAsync(new RaidListRegisterEvent());
                break;
            case RlClientPacketType.UNREGISTER:
                await session.EmitEventAsync(new RaidListUnregisterEvent());
                break;
            case RlClientPacketType.JOIN:
                await session.EmitEventAsync(new RaidListJoinEvent(rlPacket.CharacterName));
                break;
        }

        await session.EmitEventAsync(new RaidListOpenEvent());
    }
}