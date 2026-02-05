using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateJoinTeamEventHandler : IAsyncEventProcessor<MateJoinTeamEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLang;
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public MateJoinTeamEventHandler(IGameLanguageService gameLang, IBuffFactory buffFactory, ISpPartnerConfiguration spPartnerConfiguration, IMateBuffConfigsContainer mateBuffConfigsContainer)
    {
        _gameLang = gameLang;
        _buffFactory = buffFactory;
        _spPartnerConfiguration = spPartnerConfiguration;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
    }

    public async Task HandleAsync(MateJoinTeamEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        if (mateEntity == null)
        {
            return;
        }

        if (!e.IsOnCharacterEnter)
        {
            if (mateEntity.IsTeamMember)
            {
                return;
            }
        }

        if (!session.IsGameMaster())
        {
            if (mateEntity.Level > session.PlayerEntity.Level)
            {
                session.SendMsg(_gameLang.GetLanguage(GameDialogKey.PET_SHOUTMESSAGE_HIGHER_LEVEL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.GetDignityIco() == 6)
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.PET_SHOUTMESSAGE_DIGNITY_LOW), MsgMessageType.Middle);
                return;
            }
        }

        mateEntity.IsTeamMember = true;
        if (e.IsOnCharacterEnter)
        {
            mateEntity.TeleportNearCharacter();
        }
        else if (!e.IsNewCreated)
        {
            mateEntity.ChangePosition(new Position(mateEntity.MapX, mateEntity.MapY));
        }

        session.SendScpStcPacket();

        switch (mateEntity.MateType)
        {
            case MateType.Pet:
                if (mateEntity.HasDhaPremium)
                {
                    var newSkill = new CharacterSkill { SkillVNum = (short)SkillsVnums.DHA_PREMIUM };
                    if (session.PlayerEntity.UseSp)
                    {
                        session.PlayerEntity.SkillsSp[(short)SkillsVnums.DHA_PREMIUM] = newSkill;
                    }
                    else
                    {
                        session.PlayerEntity.CharacterSkills[(short)SkillsVnums.DHA_PREMIUM] = newSkill;
                    }
                    session.PlayerEntity.Skills.Add(newSkill);
                    session.RefreshSkillList();
                    session.RefreshQuicklist();
                }
                await session.AddPetBuff(mateEntity, _mateBuffConfigsContainer, _buffFactory);
                session.SendMateSkillPacket(mateEntity);
                session.SendMateSkillCooldown(mateEntity);
                break;
            case MateType.Partner when mateEntity.Skills is not null:
                foreach (IBattleEntitySkill skill in mateEntity.Skills.Where(s => s.Skill.CastId is 38 or 39 && mateEntity.Level >= s.Skill.LevelMinimum))
                {
                    var newSkill = new CharacterSkill { SkillVNum = skill.Skill.Id };
                    if (session.PlayerEntity.UseSp)
                    {
                        session.PlayerEntity.SkillsSp[skill.Skill.Id] = newSkill;
                    }
                    else
                    {
                        session.PlayerEntity.CharacterSkills[skill.Skill.Id] = newSkill;
                    }
                    session.PlayerEntity.Skills.Add(newSkill);
                }
                session.RefreshSkillList();
                session.RefreshQuicklist();
                break;
        }

        if (!e.IsOnCharacterEnter)
        {
            session.PlayerEntity.MapInstance.AddMate(e.MateEntity);
            e.MateEntity.MapInstance.Broadcast(s => e.MateEntity.GenerateIn(_gameLang, s.UserLanguage, _spPartnerConfiguration));
        }

        session.SendCondMate(mateEntity);
        session.SendPClearPacket();
        session.SendScnPackets();
        session.SendScpPackets();
        session.RefreshParty(_spPartnerConfiguration);
    }
}