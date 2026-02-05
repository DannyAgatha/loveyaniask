using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Utility;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class SuctlPacketHandler : GenericGamePacketHandlerBase<SuctlPacket>
{
    private readonly IPartnerSpecialistBasicConfig _partnerSpecialistBasic;
    private readonly IRandomGenerator _randomGenerator;

    public SuctlPacketHandler(IRandomGenerator randomGenerator, IPartnerSpecialistBasicConfig partnerSpecialistBasic)
    {
        _randomGenerator = randomGenerator;
        _partnerSpecialistBasic = partnerSpecialistBasic;
    }

    protected override async Task HandlePacketAsync(IClientSession session, SuctlPacket packet)
    {
        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if (packet == null)
        {
            session.SendDebugMessage("packet == null");
            return;
        }

        // check, if owner of mate is in PvP cell
        if (session.CurrentMapInstance.IsPvp && session.CurrentMapInstance.PvpZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY))
        {
            session.SendDebugMessage("Character in No-PvP zone");
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendDebugMessage("Mate - session.IsVehicled");
            return;
        }

        if (session.PlayerEntity.Invisible)
        {
            session.SendDebugMessage("Mate - session.PlayerEntity.Invisible");
            return;
        }

        if (session.PlayerEntity.IsInvisible())
        {
            session.SendDebugMessage("Mate - session.PlayerEntity.IsInvisible()");
            return;
        }

        if (session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.SendDebugMessage("Mate - session.IsVehicled, InvisibleGm");
            return;
        }

        if (session.PlayerEntity.IsInvisible())
        {
            return;
        }

        INpcEntity npc = session.PlayerEntity.MapInstance.GetNpcById(packet.MateTransportId);
        if (npc != null)
        {
            await CheckNpcAttack(session, npc, packet);
            return;
        }

        IMateEntity attacker = session.PlayerEntity.MateComponent.GetMate(x => x.Id == packet.MateTransportId);

        if (attacker == null)
        {
            session.SendDebugMessage("Mate == null");
            return;
        }

        if (!attacker.IsAlive())
        {
            session.SendDebugMessage("Mate is dead");
            return;
        }

        if (attacker.IsCastingSkill)
        {
            session.SendDebugMessage("Mate IsCasting");
            return;
        }

        if (attacker.IsSitting)
        {
            session.SendDebugMessage("Mate IsSitting");
            return;
        }

        if (!attacker.CanAttack())
        {
            return;
        }

        // check, if mate is in PvP cell
        if (attacker.MapInstance.IsPvp && attacker.MapInstance.PvpZone(attacker.PositionX, attacker.PositionY))
        {
            session.SendDebugMessage("Mate in No-PvP zone");
            return;
        }

        IBattleEntity target = session.PlayerEntity.MapInstance.GetBattleEntity(packet.TargetType, packet.TargetId);

        switch (target)
        {
            case null:
                session.SendDebugMessage("Target == null");
                return;
            case IMonsterEntity { MonsterRaceType: MonsterRaceType.Fixed }:
            case IPlayerEntity { IsSeal: true }:
                return;
        }

        // check, if target is in PvP cell
        if (target.MapInstance.IsPvp && target.MapInstance.PvpZone(target.PositionX, target.PositionY))
        {
            session.SendDebugMessage("Target in No-PvP zone");
            return;
        }

        // check, if target owner is in PvP cell
        IPlayerEntity targetMateOwner = (target as IMateEntity)?.Owner;
        if (targetMateOwner != null && target.MapInstance.PvpZone(targetMateOwner.PositionX, targetMateOwner.PositionY) && target.MapInstance.IsPvp)
        {
            session.SendDebugMessage("Target Mate Owner in No-PvP zone");
            return;
        }

        if (!target.IsAlive())
        {
            session.SendDebugMessage("Target is dead");
            return;
        }

        if (target is IMonsterEntity mob && mob.SummonerId != 0 && mob.SummonerType == VisualType.Player && !mob.IsMateTrainer && !mob.IsSparringMonster)
        {
            return;
        }

        IEnumerable<IBattleEntitySkill> mateSkills = attacker.Skills;

        if (mateSkills == null)
        {
            session.SendDebugMessage("mateSkills == null");
            return;
        }

        IBattleEntitySkill ski = mateSkills.FirstOrDefault(s => s.Skill.SkillType == SkillType.MonsterSkill);
        SkillDTO skill = null;
        if (ski is NpcMonsterSkill npcMonsterSkill)
        {
            if (npcMonsterSkill.IsBasicAttack)
            {
                skill = npcMonsterSkill.Skill;
            }
            else if (npcMonsterSkill.Rate == 0 || _randomGenerator.RandomNumber() <= npcMonsterSkill.Rate)
            {
                skill = ski.Skill;
            }
        }

        SkillInfo skillInfo = skill == null ? attacker.BasicSkill.DeepClone() : skill.GetInfo(battleEntity: attacker);

        if (attacker.IsUsingSp && attacker.Specialist != null)
        {
            skillInfo.Element = attacker.Specialist.GameItem.Element;
        }

        if (skillInfo.Vnum != 0 && !attacker.SkillCanBeUsed(ski, DateTime.UtcNow))
        {
            skill = null;
            skillInfo = attacker.BasicSkill;
        }

        if (session.PlayerEntity.IsAllyWith(target))
        {
            return;
        }

        if (attacker.Mp < skill?.MpCost)
        {
            skill = null;
            skillInfo = attacker.BasicSkill;
        }

        if (skillInfo.Vnum == 0 && attacker.LastBasicSkill > DateTime.UtcNow)
        {
            return;
        }

        if (!session.PlayerEntity.CheatComponent.HasGodMode && skill != null)
        {
            attacker.RemoveEntityMp(skill.MpCost, skill);
        }

        int cellSizeBonus = 3;

        if (target is INpcMonsterEntity npcMonsterEntity)
        {
            cellSizeBonus += npcMonsterEntity.CellSize;
        }

        if (!attacker.Position.IsInRange(target.Position, skillInfo.Range + cellSizeBonus))
        {
            return;
        }

        session.SendMateEffect(attacker, EffectType.PetAttack);
        session.SendMateLife(attacker);
        DateTime castTime = attacker.GenerateSkillCastTime(skillInfo);
        attacker.LastSkillUse = DateTime.UtcNow;
        if (skillInfo.Vnum == 0)
        {
            if (skillInfo.Cooldown > 10) // Cooldown is > 1 second
            {
                skillInfo.Cooldown = 10;
            }

            attacker.LastBasicSkill = DateTime.UtcNow.AddMilliseconds(attacker.ApplyCooldownReduction(skillInfo) * 100);

            if (attacker.IsUsingSp && attacker.Specialist != null)
            {
                skillInfo.HitEffect = _partnerSpecialistBasic.GetAttackEffect(attacker.Specialist.GameItem.Morph);
            }
        }

        await attacker.EmitEventAsync(new BattleExecuteSkillEvent(attacker, target, skillInfo, castTime));
    }

    private async Task CheckNpcAttack(IClientSession session, INpcEntity npc, SuctlPacket packet)
    {
        if (npc.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (!npc.IsTimeSpaceMate)
        {
            return;
        }

        List<INpcEntity> partners = session.PlayerEntity.TimeSpaceComponent.Partners;
        INpcEntity partner = partners.FirstOrDefault(x => x.MonsterVNum == npc.MonsterVNum);
        if (partner == null || !npc.CharacterPartnerId.HasValue || npc.Id != partner.Id)
        {
            return;
        }

        if (!npc.IsAlive())
        {
            session.SendDebugMessage("Mate is dead");
            return;
        }

        if (npc.IsCastingSkill)
        {
            session.SendDebugMessage("Mate IsCasting");
            return;
        }

        if (npc.IsSitting)
        {
            session.SendDebugMessage("Mate IsSitting");
            return;
        }

        if (!npc.CanPerformAttack())
        {
            return;
        }

        IBattleEntity target = session.PlayerEntity.MapInstance.GetBattleEntity(packet.TargetType, packet.TargetId);

        switch (target)
        {
            case null:
                session.SendDebugMessage("Target == null");
                return;
            case IMonsterEntity { MonsterRaceType: MonsterRaceType.Fixed }:
                return;
        }

        if (!target.IsAlive())
        {
            session.SendDebugMessage("Target is dead");
            return;
        }

        if (target is IMonsterEntity mob && mob.SummonerId != 0 && mob.SummonerType == VisualType.Player)
        {
            return;
        }

        IBattleEntitySkill skillToUse = null;
        DateTime now = DateTime.UtcNow;

        foreach (IBattleEntitySkill skill in npc.Skills)
        {
            if (skill is not NpcMonsterSkill npcMonsterSkill)
            {
                continue;
            }

            if (npcMonsterSkill.IsBasicAttack && npc.SkillCanBeUsed(npcMonsterSkill, now))
            {
                skillToUse = npcMonsterSkill;
                break;
            }

            if (!npc.SkillCanBeUsed(npcMonsterSkill, now))
            {
                continue;
            }

            skillToUse = npcMonsterSkill;
            break;
        }

        SkillInfo skillInfo = skillToUse?.Skill.GetInfo() ?? npc.BasicSkill;

        if (session.PlayerEntity.IsAllyWith(target))
        {
            return;
        }

        if (skillInfo.Vnum == 0 && npc.Skills.Any(x => x is NpcMonsterSkill { IsBasicAttack: true }))
        {
            return;
        }

        if (npc.LastBasicAttack > DateTime.UtcNow && skillInfo.Vnum == 0)
        {
            return;
        }

        int cellSizeBonus = 3;

        if (target is INpcMonsterEntity npcMonsterEntity)
        {
            cellSizeBonus += npcMonsterEntity.CellSize;
        }

        if (!npc.Position.IsInRange(target.Position, skillInfo.Range + cellSizeBonus))
        {
            return;
        }

        session.SendNpcEffect(npc, EffectType.PetAttack);
        DateTime castTime = npc.GenerateSkillCastTime(skillInfo);

        npc.LastBasicAttack = DateTime.UtcNow.AddMilliseconds(npc.ApplyCooldownReduction(npc.BasicSkill) * 100);
        await npc.EmitEventAsync(new BattleExecuteSkillEvent(npc, target, skillInfo, castTime));
    }
}