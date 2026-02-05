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
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class UpsPacketHandler : GenericGamePacketHandlerBase<UpsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, UpsPacket packet)
    {
        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if (!session.PlayerEntity.CanFight() || packet == null)
        {
            session.SendDebugMessage("[U_PS] !CanFight or packet == null");
            return;
        }

        // check, if owner of mate is in PvP cell
        if (session.CurrentMapInstance.IsPvp && session.CurrentMapInstance.PvpZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY))
        {
            session.SendDebugMessage("[U_PS] Character in No-PvP zone");
            return;
        }

        if (packet.SkillSlot > 2)
        {
            session.SendDebugMessage("[U_PS] skillSlot > 2");
            return;
        }

        IMateEntity attacker = session.PlayerEntity.MateComponent.GetMate(x => x.Id == packet.MateTransportId && x.MateType == MateType.Partner);

        if (attacker == null)
        {
            session.SendDebugMessage("[U_PS] attacker == null");
            return;
        }

        if (!attacker.IsUsingSp)
        {
            session.SendDebugMessage("[U_PS] !attacker.IsUsingSp");
            return;
        }

        if (!attacker.IsAlive())
        {
            session.SendDebugMessage("[U_PS] attacker is dead");
            return;
        }

        if (attacker.IsCastingSkill)
        {
            session.SendDebugMessage("[U_PS] Mate IsCasting");
            return;
        }

        if (attacker.IsSitting)
        {
            session.SendDebugMessage("[U_PS] Mate IsSitting");
            return;
        }

        if (!attacker.CanAttack())
        {
            session.SendDebugMessage("[U_PS] !attacker.CanAttack");
            return;
        }

        // check, if mate is in PvP cell
        if (attacker.MapInstance.IsPvp && attacker.MapInstance.PvpZone(attacker.PositionX, attacker.PositionY))
        {
            session.SendDebugMessage("[U_PS] Mate in No-PvP zone");
            return;
        }

        if (session.PlayerEntity.IsOnVehicle || session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.SendDebugMessage("[U_PS] Mate - session.IsVehicled, InvisibleGm");
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
                session.SendDebugMessage("[U_PS] Target == null");
                return;
            case IMonsterEntity { MonsterRaceType: MonsterRaceType.Fixed }:
            case IPlayerEntity { IsSeal: true }:
                return;
        }

        // check, if target is in PvP cell
        if (target.MapInstance.IsPvp && target.MapInstance.PvpZone(target.PositionX, target.PositionY))
        {
            session.SendDebugMessage("[U_PS] Target in No-PvP zone");
            return;
        }

        // check, if target owner is in PvP cell
        IPlayerEntity targetMateOwner = (target as IMateEntity)?.Owner;
        if (targetMateOwner != null && target.MapInstance.PvpZone(targetMateOwner.PositionX, targetMateOwner.PositionY) && target.MapInstance.IsPvp)
        {
            session.SendDebugMessage("[U_PS] Target Mate Owner in No-PvP zone");
            return;
        }

        if (!target.IsAlive())
        {
            session.SendDebugMessage("[U_PS] Target is dead");
            return;
        }

        List<PartnerSkill> mateSkills = attacker.Specialist.PartnerSkills;
        PartnerSkill skill = mateSkills?.FirstOrDefault(s => s.Skill.CastId == packet.SkillSlot);

        if (skill == null)
        {
            session.SendDebugMessage("[U_PS] skill == null");
            return;
        }

        if (!attacker.SkillCanBeUsed(skill))
        {
            session.SendDebugMessage("[U_PS] !skill.CanBeUsed()");
            return;
        }

        SkillInfo skillInfo = skill.Skill.GetInfo(skill, attacker);
        if (attacker.IsUsingSp && attacker.Specialist != null)
        {
            skillInfo.Element = attacker.Specialist.GameItem.Element;
        }

        if (!attacker.IsEnemyWith(target) && attacker.IsMate() && skillInfo.TargetType != TargetType.Self)
        {
            session.SendDebugMessage("[U_PS] !attacker.IsEnemyWith(target)");
            return;
        }

        if (attacker.Mp < skill.Skill.MpCost)
        {
            session.SendDebugMessage("[U_PS] attacker.Mp < MpCost");
            return;
        }

        Position newPosition = default;
        if (skillInfo.AttackType == AttackType.Dash)
        {
            if (!attacker.MapInstance.IsBlockedZone(packet.MapX, packet.MapY))
            {
                newPosition = new Position(packet.MapX, packet.MapY);
            }

            if (newPosition == default || !attacker.Position.IsInRange(newPosition, skillInfo.Range + 3))
            {
                newPosition = target.Position;
            }
        }


        int cellSizeBonus = 3;

        if (target is INpcMonsterEntity npcMonsterEntity)
        {
            cellSizeBonus += npcMonsterEntity.CellSize;
        }

        if (!attacker.Position.IsInRange(target.Position, skillInfo.Range + cellSizeBonus))
        {
            session.SendDebugMessage("[U_PS] !attacker.IsInRange");
            return;
        }

        if (!session.PlayerEntity.CheatComponent.HasGodMode)
        {
            attacker.RemoveEntityMp(skill.Skill.MpCost, skill.Skill);
        }

        attacker.TargetVisualType = target.Type;
        attacker.TargetId = target.Id;

        DateTime castTime = attacker.GenerateSkillCastTime(skillInfo, true);
        attacker.LastUsedPartnerSkill = skill;
        session.SendDebugMessage("[U_PS] Casting u_ps");
        
        Position castPosition = skillInfo.AttackType == AttackType.Dash
            ? newPosition
            : target.Position;

        await attacker.EmitEventAsync(new BattleExecuteSkillEvent(attacker, target, skillInfo, castTime, castPosition));
    }
}