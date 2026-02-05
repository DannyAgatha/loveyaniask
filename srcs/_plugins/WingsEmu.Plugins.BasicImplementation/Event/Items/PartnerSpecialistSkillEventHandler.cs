using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class PartnerSpecialistSkillEventHandler : IAsyncEventProcessor<PartnerSpecialistSkillEvent>
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IPartnerSpecialistSkillRoll _partnerSpecialistSkill;
    private readonly ISkillsManager _skillsManager;

    public PartnerSpecialistSkillEventHandler(ISkillsManager skillsManager, IGameLanguageService languageService, IDelayManager delayManager, IPartnerSpecialistSkillRoll partnerSpecialistSkill)
    {
        _skillsManager = skillsManager;
        _gameLanguage = languageService;
        _delayManager = delayManager;
        _partnerSpecialistSkill = partnerSpecialistSkill;
    }

    public async Task HandleAsync(PartnerSpecialistSkillEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        byte partnerSlot = e.PartnerSlot;
        byte skillSlot = e.SkillSlot;
        bool roll = e.Roll;

        IMateEntity partnerInTeam = session.PlayerEntity.MateComponent.GetTeamMember(s => s.MateType == MateType.Partner && s.PetSlot == partnerSlot);
        if (partnerInTeam == null)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_NO_PARTNER_IN_TEAM, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (!partnerInTeam.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (skillSlot > 2)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to learn PSP skill when slot > 2");
            return;
        }

        if (partnerInTeam.Specialist == null)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_NO_SP_EQUIPPED, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partnerInTeam.IsUsingSp)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_IS_WEARING_SP, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partnerInTeam.Level < 30)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_LEVEL_IS_TOO_LOW, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partnerInTeam.Specialist.Agility < 100 && !session.IsGameMaster())
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_NEED_AGILITY_POINTS, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partnerInTeam.HavePartnerSkill(skillSlot))
        {
            return;
        }

        if (!roll)
        {
            DateTime waitUntil = await _delayManager.RegisterAction(partnerInTeam, DelayedActionType.PartnerLearnSkill);
            session.SendMateDelay(partnerInTeam, (int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.Identifying, $"#ps_op^{partnerSlot}^{skillSlot}^1");
            session.CurrentMapInstance?.Broadcast(partnerInTeam.GenerateMateDance(), new RangeBroadcast(partnerInTeam.PositionX, partnerInTeam.PositionY));
            return;
        }

        bool canLearn = await _delayManager.CanPerformAction(partnerInTeam, DelayedActionType.PartnerLearnSkill);
        if (!canLearn)
        {
            return;
        }

        await _delayManager.CompleteAction(partnerInTeam, DelayedActionType.PartnerLearnSkill);

        switch (skillSlot)
        {
            case 0:
                partnerInTeam.Specialist.PartnerSkill1 = true;
                break;
            case 1:
                partnerInTeam.Specialist.PartnerSkill2 = true;
                break;
            case 2:
                partnerInTeam.Specialist.PartnerSkill3 = true;
                break;
            default:
                return;
        }

        RollChances(partnerInTeam, skillSlot);

        partnerInTeam.Specialist.Agility = 0;
        session.SendMatePskiPacket(partnerInTeam);
        session.SendPetInfo(partnerInTeam, _gameLanguage);
        session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_SP_NEW_SKILL, session.UserLanguage), ModalType.Confirm);
    }

    private void RollChances(IMateEntity mateEntity, byte slot)
    {
        mateEntity.Specialist.PartnerSkills ??= new List<PartnerSkill>(3);
        foreach (SkillDTO ski in _skillsManager.GetSkills().Where(ski
                     => ski.SkillType == SkillType.PartnerSkill && ski.UpgradeType == mateEntity.Specialist.GameItem.SpMorphId && ski.CastId == slot))
        {
            mateEntity.Specialist.PartnerSkills.Add(new PartnerSkill
            {
                LastUse = DateTime.MinValue,
                Rank = _partnerSpecialistSkill.GetRandomSkillRank(),
                Slot = (byte)ski.CastId,
                SkillId = ski.Id
            });
        }
    }
}