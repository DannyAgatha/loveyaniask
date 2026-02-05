using PhoenixLib.Logging;
using System;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class HeroPacketHandler : GenericGamePacketHandlerBase<HeroPacket>
{
    private readonly IGameLanguageService _language;
    private readonly ISessionManager _sessionManager;
    private readonly IRankingManager _rankingManager;

    public HeroPacketHandler(ISessionManager sessionManager, IGameLanguageService language, IRankingManager rankingManager)
    {
        _sessionManager = sessionManager;
        _language = language;
        _rankingManager = rankingManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, HeroPacket packet)
    {
        string message = packet.Message;
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        
        if (!session.IsGameMaster() && session.PlayerEntity.GetTopReputationPlace(_rankingManager.TopReputation) > 3)
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_USER_NOT_HERO, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        message = message.Trim();
        if (message.Length > 60)
        {
            message = message.Substring(0, 60);
        }

        await _sessionManager.BroadcastAsync(async x => x.GenerateMsgPacket($"[{session.PlayerEntity.Name}]: {message}", MsgMessageType.BottomRed), new SpeakerHeroBroadcast());

        await session.EmitEventAsync(new ChatGenericEvent
        {
            Message = message,
            ChatType = ChatType.HeroChat
        });
    }
}