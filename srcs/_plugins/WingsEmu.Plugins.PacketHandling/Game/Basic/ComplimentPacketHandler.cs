using System;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class ComplimentPacketHandler : GenericGamePacketHandlerBase<ComplimentPacket>
{
    private readonly IComplimentsManager _complimentsManager;
    private readonly IGameLanguageService _language;
    private readonly ISessionManager _sessionManager;

    public ComplimentPacketHandler(IGameLanguageService language, ISessionManager sessionManager, IComplimentsManager complimentsManager)
    {
        _sessionManager = sessionManager;
        _complimentsManager = complimentsManager;
        _language = language;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ComplimentPacket complimentPacket)
    {
        if (complimentPacket == null)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        long complimentedCharacterId = complimentPacket.CharacterId;
        if (session.PlayerEntity.Level <= 30)
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.COMMEND_CHATMESSAGE_NOT_MINLVL, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        if (session.PlayerEntity.GameStartDate.AddMinutes(60) > DateTime.UtcNow)
        {
            session.SendChatMessage(
                _language.GetLanguageFormat(GameDialogKey.COMMEND_CHATMESSAGE_LOGIN_COOLDOWN, session.UserLanguage, (session.PlayerEntity.GameStartDate.AddMinutes(60) - DateTime.UtcNow).Minutes),
                ChatMessageColorType.Red);

            return;
        }


        IClientSession complimentedSession = _sessionManager.GetSessionByCharacterId(complimentedCharacterId);
        if (complimentedSession?.PlayerEntity == null)
        {
            return;
        }

        bool canCompliment = await _complimentsManager.CanCompliment(session.Account.Id);
        if (!canCompliment)
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.COMMEND_CHATMESSAGE_COOLDOWN, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }
        
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.Commend));
        complimentedSession.PlayerEntity.Compliment += 1;
        int baseExperience = session.PlayerEntity.SubClass.IsPveSubClass() ? 300 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 150 : 0;
        session.AddTierExperience(baseExperience, _language, false);
        session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.COMMEND_CHATMESSAGE_GIVEN, complimentedSession.PlayerEntity.Name), ChatMessageColorType.Green);
        complimentedSession.SendChatMessage(complimentedSession.GetLanguageFormat(GameDialogKey.COMMEND_CHATMESSAGE_RECEIVED, session.PlayerEntity.Name), ChatMessageColorType.LightPurple);
    }
}