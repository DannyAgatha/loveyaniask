using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
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

public class UpetPacketHandler : GenericGamePacketHandlerBase<UpetPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, UpetPacket packet)
    {
        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if (!session.PlayerEntity.CanFight() || packet == null)
        {
            session.SendDebugMessage("[U_PET] !CanFight or packet == null");
            return;
        }

        // check, if owner of mate is in PvP cell
        if (session.CurrentMapInstance.IsPvp && session.CurrentMapInstance.PvpZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY))
        {
            session.SendDebugMessage("[U_PET] Character in No-PvP zone");
            return;
        }

        IMateEntity attacker = session.PlayerEntity.MateComponent.GetMate(x => x.Id == packet.MateTransportId && x.MateType == MateType.Pet);

        if (attacker == null)
        {
            session.SendDebugMessage("[U_PET] Mate == null");
            return;
        }

        if (!attacker.IsAlive())
        {
            session.SendDebugMessage("[U_PET] Mate is dead");
            return;
        }

        if (attacker.IsCastingSkill)
        {
            session.SendDebugMessage("[U_PET] Mate IsCasting");
            return;
        }

        if (attacker.IsSitting)
        {
            session.SendDebugMessage("[U_PET] Mate IsSitting");
            return;
        }

        if (!attacker.CanAttack())
        {
            session.SendDebugMessage("[U_PET] !canAttack()");
            return;
        }

        // check, if mate is in PvP cell
        if (attacker.MapInstance.IsPvp && attacker.MapInstance.PvpZone(attacker.PositionX, attacker.PositionY))
        {
            session.SendDebugMessage("[U_PET] Mate in No-PvP zone");
            return;
        }

        if (session.PlayerEntity.IsOnVehicle || session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.SendDebugMessage("[U_PET] Mate - session.IsVehicled, InvisibleGm");
            return;
        }

        if (session.PlayerEntity.IsInvisible())
        {
            return;
        }

        IBattleEntity target = session.PlayerEntity.MapInstance.GetBattleEntity(packet.TargetType, packet.TargetId);

        switch (target)
        {
            case null:
                session.SendDebugMessage("[U_PET] Target == null");
                return;
            case IMonsterEntity { MonsterRaceType: MonsterRaceType.Fixed }:
            case IPlayerEntity { IsSeal: true }:
                return;
        }

        // check, if target is in PvP cell
        if (target.MapInstance.IsPvp && target.MapInstance.PvpZone(target.PositionX, target.PositionY))
        {
            session.SendDebugMessage("[U_PET] Target in No-PvP zone");
            return;
        }

        // check, if target owner is in PvP cell
        IPlayerEntity targetMateOwner = (target as IMateEntity)?.Owner;
        if (targetMateOwner != null && target.MapInstance.PvpZone(targetMateOwner.PositionX, targetMateOwner.PositionY) && target.MapInstance.IsPvp)
        {
            session.SendDebugMessage("[U_PET] Target Mate Owner in No-PvP zone");
            return;
        }

        if (!target.IsAlive())
        {
            session.SendDebugMessage("[U_PET] Target is dead");
            return;
        }

        IEnumerable<IBattleEntitySkill> mateSkills = attacker.Skills;

        if (mateSkills == null)
        {
            session.SendDebugMessage("[U_PET] mateSkills == null");
            return;
        }

        IBattleEntitySkill ski = attacker.SpecialSkillVnum.HasValue ? mateSkills.FirstOrDefault(s => s.Skill.Id == attacker.SpecialSkillVnum.Value && s.Skill.SkillType == SkillType.PartnerSkill && s.Skill.Id == packet.SkillVnum)
            : packet.SkillVnum == 0 ? mateSkills.FirstOrDefault(s => s.Skill.SkillType == SkillType.PartnerSkill && s.Skill.Id == packet.SkillVnum) 
            : mateSkills.FirstOrDefault(s => s.Skill.Id == packet.SkillVnum && s.Skill.SkillType == SkillType.PartnerSkill && s.Skill.Id == packet.SkillVnum);

        if (ski == null)
        {
            session.SendDebugMessage("[U_PET] skill == null");
            return;
        }

        SkillDTO skill = ski.Skill;
        SkillInfo skillInfo = skill.GetInfo(battleEntity: attacker);
        
        if (attacker.TrainerSkills.Contains(ski.Skill.Id))
        {
            int index = attacker.TrainerSkills.IndexOf(ski.Skill.Id);
            ski.Skill.CastId = (short)(index + 1);
            skill.CastId = (short)(index + 1);
            skillInfo.CastId = (short)(index + 1);
        }
        else
        {
            ski.Skill.CastId = 0;
            skill.CastId = 0;
            skillInfo.CastId = 0;
        }

        if (!attacker.SkillCanBeUsed(ski, DateTime.UtcNow))
        {
            session.SendDebugMessage("[U_PET] !skill.CanBeUsed()");
            return;
        }

        if (!attacker.IsEnemyWith(target) && attacker.IsMate() && skillInfo.TargetType != TargetType.Self)
        {
            session.SendDebugMessage("[U_PET] !attacker.IsEnemyWith(target)");
            return;
        }

        if (attacker.Mp < skill.MpCost)
        {
            session.SendDebugMessage("[U_PET] attacker.Mp < MpCost");
            return;
        }

        if (!session.PlayerEntity.CheatComponent.HasGodMode)
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
            session.SendDebugMessage("[U_PET] !attacker.IsInRange");
            return;
        }

        attacker.LastSkillUse = DateTime.UtcNow;
        DateTime castTime = attacker.GenerateSkillCastTime(skillInfo);
        session.SendDebugMessage("[U_PET] Casting u_pet");

        await attacker.EmitEventAsync(new BattleExecuteSkillEvent(attacker, target, skillInfo, castTime, target.Position));
    }
}