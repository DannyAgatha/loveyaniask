using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateSpTransformEventHandler : IAsyncEventProcessor<MateSpTransformEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillsManager _skillsManager;
    private readonly ISpPartnerConfiguration _spPartner;

    public MateSpTransformEventHandler(IGameLanguageService gameLanguage, ISkillsManager skillsManager, IBuffFactory buffFactory, ISpPartnerConfiguration spPartner)
    {
        _gameLanguage = gameLanguage;
        _skillsManager = skillsManager;
        _buffFactory = buffFactory;
        _spPartner = spPartner;
    }

    public async Task HandleAsync(MateSpTransformEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;
        IClientSession session = e.Sender;

        if (mateEntity.Specialist == null)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_NO_SP_EQUIPPED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await mateEntity.RemoveAllBuffsAsync(false);
        mateEntity.IsUsingSp = true;
        session.PlayerEntity.ClearMateSkillCooldowns();

        if (mateEntity.Specialist.PartnerSkills != null)
        {
            mateEntity.Skills.AddRange(mateEntity.Specialist.PartnerSkills.Where(partnerSkill => partnerSkill != null));
        }
        
        session.SendCondMate(mateEntity);
        session.Broadcast(mateEntity.GenerateCMode(mateEntity.Specialist.GameItem.Morph));
        session.SendMatePskiPacket(mateEntity);
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.Broadcast(mateEntity.GenerateOut());
        session.CurrentMapInstance.Broadcast(s => mateEntity.GenerateIn(_gameLanguage, s.UserLanguage, _spPartner));
        session.RefreshParty(_spPartner);
        session.SendMateEffect(mateEntity, EffectType.Transform);
        session.CurrentMapInstance.Broadcast(session.GenerateGuriPacket(6, 2, mateEntity.Id, 10));

        int sum = 0;
        foreach (IBattleEntitySkill skill in mateEntity.Skills)
        {
            if (skill is not PartnerSkill partnerSkill)
            {
                continue;
            }

            sum += partnerSkill.Rank;
        }

        if (sum == 0)
        {
            sum = 1;
        }
        else
        {
            sum /= 3;
        }

        if (sum < 1)
        {
            sum = 1;
        }

        SpPartnerInfo spInfo = _spPartner.GetByMorph(mateEntity.Specialist.GameItem.Morph);

        if (spInfo == null)
        {
            return;
        }

        if (spInfo.BuffId == 0)
        {
            return;
        }

        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(spInfo.BuffId + (sum - 1), session.PlayerEntity, BuffFlag.PARTNER));
    }
}