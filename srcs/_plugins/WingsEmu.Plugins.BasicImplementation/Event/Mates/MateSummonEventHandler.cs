using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateSummonEventHandler : IAsyncEventProcessor<MateSummonEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;
    private readonly ISpPartnerConfiguration _partnerConfiguration;

    public MateSummonEventHandler(IGameLanguageService gameLanguage, IMateBuffConfigsContainer mateBuffConfigsContainer, ISpPartnerConfiguration partnerConfiguration, IBuffFactory buffFactory)
    {
        _gameLanguage = gameLanguage;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
        _partnerConfiguration = partnerConfiguration;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(MateSummonEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;
        IClientSession session = e.Sender;

        if (mateEntity == null)
        {
            return;
        }

        if (!mateEntity.IsSummonable)
        {
            return;
        }

        if (session.PlayerEntity.MapInstance.Id == session.PlayerEntity.Miniland.Id)
        {
            return;
        }

        if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle 
            || session.PlayerEntity.MapInstance.MapInstanceType is MapInstanceType.UnderWaterShowdown)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        if (!session.IsGameMaster())
        {
            if (mateEntity.Level > session.PlayerEntity.Level)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PET_SHOUTMESSAGE_HIGHER_LEVEL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.GetDignityIco() == 6)
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.PET_SHOUTMESSAGE_DIGNITY_LOW), MsgMessageType.Middle);
                return;
            }
        }

        if (e.Sender.PlayerEntity.MateComponent.GetMate(s => s.IsTeamMember && s.MateType == mateEntity.MateType) != null)
        {
            e.Sender.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_ALREADY_IN_TEAM, e.Sender.UserLanguage), ChatMessageColorType.Red);
            e.Sender.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_ALREADY_IN_TEAM, e.Sender.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (mateEntity.IsTeamMember)
        {
            return;
        }

        if (session.PlayerEntity.Miniland.Sessions.Any())
        {
            mateEntity.MapInstance.Broadcast(mateEntity.GenerateOut());
        }

        session.PlayerEntity.Miniland.RemoveMate(mateEntity);
        mateEntity.IsTeamMember = true;
        session.SendScpStcPacket();

        switch (mateEntity.MateType)
        {
            case MateType.Pet:
                if (mateEntity.HasDhaPremium)
                {
                    var newSkill = new CharacterSkill { SkillVNum = (short)SkillsVnums.DHA_PREMIUM };
                    session.PlayerEntity.CharacterSkills[(short)SkillsVnums.DHA_PREMIUM] = newSkill;
                    session.PlayerEntity.Skills.Add(newSkill);
                    session.RefreshSkillList();
                    session.RefreshQuicklist();
                }
                await session.AddPetBuff(mateEntity, _mateBuffConfigsContainer, _buffFactory);
                session.SendMateSkillPacket(mateEntity);
                session.SendMateSkillCooldown(mateEntity);
                break;
            case MateType.Partner when mateEntity.Skills?.Count != 0:
                session.RefreshSkillList();
                session.RefreshQuicklist();
                break;
        }

        session.PlayerEntity.MapInstance.AddMate(e.MateEntity);
        mateEntity.TeleportNearCharacter();
        session.CurrentMapInstance.Broadcast(s => e.MateEntity.GenerateIn(_gameLanguage, s.UserLanguage, _partnerConfiguration));
        session.SendCondMate(mateEntity);
        session.SendScnPackets();
        session.SendScpPackets();
        session.SendPClearPacket();
        session.SendScnPackets();
        session.SendScpPackets();
        session.RefreshParty(_partnerConfiguration);
    }
}