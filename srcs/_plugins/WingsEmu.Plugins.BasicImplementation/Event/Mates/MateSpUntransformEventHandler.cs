using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateSpUntransformEventHandler : IAsyncEventProcessor<MateSpUntransformEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartner;

    public MateSpUntransformEventHandler(IAsyncEventPipeline eventPipeline, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartner)
    {
        _eventPipeline = eventPipeline;
        _gameLanguage = gameLanguage;
        _spPartner = spPartner;
    }

    public async Task HandleAsync(MateSpUntransformEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;
        IClientSession session = e.Sender;

        if (mateEntity.Specialist == null)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_NO_SP_EQUIPPED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (mateEntity.Specialist.PartnerSkills != null)
        {
            foreach (PartnerSkill partnerSkill in mateEntity.Specialist.PartnerSkills)
            {
                partnerSkill.LastUse = DateTime.MinValue;
            }
        }

        await _eventPipeline.ProcessEventAsync(new BuffRemoveEvent
        {
            Entity = session.PlayerEntity,
            Buffs = session.PlayerEntity.BuffComponent.GetAllBuffs(x => x.BuffFlags == BuffFlag.PARTNER && x.Caster.GetMateType() != MateType.Pet),
            RemovePermanentBuff = true
        }, cancellation);

        short cooldown = 10;
        mateEntity.IsUsingSp = false;
        
        if (mateEntity.Specialist.PartnerSkills != null)
        {
            foreach (PartnerSkill partnerSkill in mateEntity.Specialist.PartnerSkills)
            {
                mateEntity.Skills.Remove(partnerSkill);
            }
        }
        
        await mateEntity.RemoveAllBuffsAsync(false);
        mateEntity.SpCooldownEnd = DateTime.UtcNow.AddSeconds(cooldown);
        session.PlayerEntity.ClearMateSkillCooldowns();
        session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_PSP_STAY_TIME, session.UserLanguage, cooldown), ChatMessageColorType.Red);
        session.SendCondMate(mateEntity);
        session.Broadcast(mateEntity.GenerateCMode(-1));
        session.SendRemoveMateSpSkills(mateEntity);
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.Broadcast(mateEntity.GenerateOut());
        session.CurrentMapInstance.Broadcast(s => mateEntity.GenerateIn(_gameLanguage, s.UserLanguage, _spPartner));
        session.RefreshParty(_spPartner);
        session.SendMateSpCooldown(mateEntity, cooldown);
        mateEntity.TeleportNearCharacter();
    }
}