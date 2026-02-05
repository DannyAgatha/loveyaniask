using PhoenixLib.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpTransformEventHandler : IAsyncEventProcessor<SpTransformEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _languageService;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISkillsManager _skillsManager;
    private readonly ISpWingConfiguration _spWingConfiguration;
    private readonly IMapManager _mapManager;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly ICardsManager _cardsManager;

    public SpTransformEventHandler(IGameLanguageService languageService, ISkillsManager skillsManager, ISpWingConfiguration spWingConfiguration, IBuffFactory buffFactory,
        ICharacterAlgorithm characterAlgorithm, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager, IMapManager mapManager, INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory, ICardsManager cardsManager)
    {
        _languageService = languageService;
        _skillsManager = skillsManager;
        _spWingConfiguration = spWingConfiguration;
        _buffFactory = buffFactory;
        _characterAlgorithm = characterAlgorithm;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _mapManager = mapManager;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _cardsManager = cardsManager;
    }

    public async Task HandleAsync(SpTransformEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance specialist = e.Specialist;
        GameItemInstance fairy = session.PlayerEntity.Fairy;

        if (!e.Forced)
        {
            if (session.PlayerEntity.IsCastingSkill)
            {
                return;
            }

            if (specialist.GameItem.IsPartnerSpecialist)
            {
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsedSp(s)))
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.CanTransformWithCooldownComplete);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if ((byte)session.PlayerEntity.GetReputationIcon(_reputationConfiguration, _rankingManager.TopReputation) < specialist.GameItem.ReputationMinimum)
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.CanNotTransformReputation);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (fairy != null && specialist.GameItem.Element != 0 && fairy.GameItem.Element != specialist.GameItem.Element && 
                !session.IsRenegadeSpecialist(specialist, fairy) && specialist.GameItem.Morph != (short)MorphType.PetTrainer && specialist.GameItem.Morph != (short)MorphType.PetTrainerSkin && 
                specialist.GameItem.Morph != (short)MorphType.Angler && specialist.GameItem.Morph != (short)MorphType.AnglerSkin && 
                specialist.GameItem.Morph != (short)MorphType.Chef && specialist.GameItem.Morph != (short)MorphType.ChefSkin)
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.SpecialistAndFairyDifferentElement);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (!session.PlayerEntity.IsSpCooldownElapsed())
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.CantTrasformWithSideEffect, 4, session.PlayerEntity.GetSpCooldown());
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }
        }

        await session.PlayerEntity.RemoveBuffsOnSpTransformAsync();

        session.PlayerEntity.BCardComponent.AddEquipmentBCards(EquipmentType.Sp, specialist.GameItem.BCards);

        session.RefreshSpPoint();
        session.PlayerEntity.LastTransform = DateTime.UtcNow;
        session.PlayerEntity.LastSkillCombo = null;
        session.PlayerEntity.UseSp = true;
        session.PlayerEntity.Morph = specialist.HoldingVNum ?? specialist.GameItem.Morph;
        session.PlayerEntity.MorphUpgrade = specialist.Upgrade;
        session.PlayerEntity.MorphUpgrade2 = specialist.Design;

        session.BroadcastCMode();
        session.RefreshLevel(_characterAlgorithm);
        session.BroadcastEffect(EffectType.Transform, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        session.PlayerEntity.SpecialistComponent.RefreshSlStats(session.PlayerEntity.Specialist.CurrentActiveSpecialistSlot);
        session.RefreshSpPoint();
        session.RefreshStatChar();
        session.RefreshStat(true);
        session.SendCondPacket();
        session.SendIncreaseRange();
        session.PlayerEntity.ChargeComponent.ResetCharge();
        session.PlayerEntity.BCardComponent.ClearChargeBCard();

        if (session.PlayerEntity.IsInRaidParty)
        {
            foreach (IClientSession s in session.PlayerEntity.Raid.Members)
            {
                s.RefreshRaidMemberList(session.PlayerEntity.Raid.IsSpecialRaid());
            }
        }

        session.PlayerEntity.SkillsSp = new ConcurrentDictionary<int, CharacterSkill>();
        session.PlayerEntity.Skills.Clear();
        foreach (SkillDTO skill in _skillsManager.GetSkills())
        {
            if (!session.PlayerEntity.Specialist.IsSpSkill(skill))
            {
                continue;
            }

            var newSkill = new CharacterSkill
            {
                SkillVNum = skill.Id
            };

            session.PlayerEntity.SkillsSp[skill.Id] = newSkill;
            session.PlayerEntity.Skills.Add(newSkill);
        }
        
        foreach (CharacterSkill tattooSkill in session.PlayerEntity.CharacterSkills.Values.Where(x => x.Skill.CastId is >= 40 and <= 44))
        {
            var newSkill = new CharacterSkill { SkillVNum = tattooSkill.Skill.Id };
            session.PlayerEntity.Skills.Add(newSkill);
        }

        IMateEntity teamMate = session.PlayerEntity.MateComponent.TeamMembers().FirstOrDefault(x => x.MateType == MateType.Partner);

        if (teamMate != null && teamMate.Skills.Any())
        {
            foreach (IBattleEntitySkill partnerSkill in teamMate.Skills.Where(s => s.Skill.CastId is 38 or 39 && teamMate.Level >= s.Skill.LevelMinimum))
            {
                if (session.PlayerEntity.CharacterSkills.TryRemove(partnerSkill.Skill.Id, out CharacterSkill removedSkill))
                {
                    var newSkill = new CharacterSkill
                    {
                        SkillVNum = removedSkill.Skill.Id
                    };

                    session.PlayerEntity.SkillsSp[removedSkill.Skill.Id] = newSkill;
                    session.PlayerEntity.Skills.Add(newSkill);
                }
            }
        }
        
        BuffVnums? buffId = specialist.GetSpecialistBuff();
        if (buffId.HasValue)
        {
            Card card = _cardsManager.GetCardByCardId((int)buffId);
            if (card?.BCards != null)
            {
                session.PlayerEntity.BCardComponent.AddEquipmentBCards(EquipmentType.Sp, card.BCards);
            }
        }

        session.AssignSubClassSkill(session.PlayerEntity.SubClass, session.PlayerEntity.TierLevel);

        session.RefreshSkillList();
        session.PlayerEntity.ClearSkillCooldowns();
        session.RefreshQuicklist();
        session.SendSpFtptPacket();
        session.SendDiscordRpcPacket();
        session.PlayerEntity.BroadcastEndDancingGuriPacket();
        
        SpWingInfo wingInfo = _spWingConfiguration.GetSpWingInfo(specialist.WingsDesign);
        if (wingInfo == null)
        {
            return;
        }

        IEnumerable<Buff> buffs = wingInfo.Buffs.Select(buff => _buffFactory.CreateBuff(buff.BuffId, session.PlayerEntity, buff.IsPermanent ? BuffFlag.NO_DURATION : BuffFlag.NORMAL));
        await session.PlayerEntity.AddBuffsAsync(buffs);
    }
}