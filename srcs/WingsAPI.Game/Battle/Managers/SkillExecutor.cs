using PhoenixLib.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Battle;

public class SkillExecutor : ISkillExecutor
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;

    public SkillExecutor(IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }
    public void ExecuteDamageSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill, Position positionBeforeDash = default)
    {
        if (IsDestroyerBomb(caster, skill))
        {
            return;
        }

        caster.BroadcastCastPacket(target, skill.Skill);
        var hitProcessable = new HitProcessable(caster, target, skill, positionBeforeDash);
        caster.MapInstance.AddCastHitRequest(hitProcessable);
    }

    public void ExecuteDamageZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position)
    {
        caster.BroadcastCastNonTarget(skill.Skill);
        var hitProcessable = new HitProcessable(caster, null, skill, position);
        caster.MapInstance.AddCastHitRequest(hitProcessable);
    }

    public void ExecuteBuffZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position)
    {
        caster.BroadcastCastNonTarget(skill.Skill);
        var buffProcessable = new BuffProcessable(caster, null, skill, position);
        caster.MapInstance.AddCastBuffRequest(buffProcessable);
    }

    public void ExecuteDebuffZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position)
    {
        caster.BroadcastCastNonTarget(skill.Skill);
        var buffProcessable = new BuffProcessable(caster, null, skill, position);
        caster.MapInstance.AddCastBuffRequest(buffProcessable);
    }

    public void ExecuteBuffSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill)
    {
        SkillInfo skillInfo = skill.Skill;
        bool isTeleport = skillInfo.Vnum == (short)SkillsVnums.ARCH_TELEPORT;

        caster.BroadcastCastPacket(target, skillInfo);

        if (isTeleport)
        {
            caster.BroadcastSuPacket(caster, skillInfo, 0, SuPacketHitMode.NoDamageSuccess);
            foreach (BCardDTO x in skillInfo.BCards)
            {
                _bCardEffectHandlerContainer.Execute(caster, caster, x, skillInfo);
            }

            return;
        }

        caster.MapInstance.AddCastBuffRequest(new BuffProcessable(caster, target, skill));
    }

    public void ExecuteDebuffSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill)
    {
        caster.BroadcastCastPacket(target, skill.Skill);
        caster.MapInstance.AddCastBuffRequest(new BuffProcessable(caster, target, skill));
    }

    private bool IsDestroyerBomb(IBattleEntity caster, SkillCast skill)
    {
        if (skill.Skill.Vnum != (short)SkillsVnums.BOMB)
        {
            return false;
        }

        if (caster is not IPlayerEntity character)
        {
            return false;
        }

        if (!character.SkillComponent.BombEntityId.HasValue)
        {
            return false;
        }

        long characterId = character.Id;
        IMonsterEntity bomb = character.MapInstance.GetAliveMonsters(x => x.Id == character.SkillComponent.BombEntityId
            && x.SummonerId == characterId && x.SummonerType == VisualType.Player && x.MonsterVNum == (short)MonsterVnum.BOMB).FirstOrDefault();
        if (bomb == null)
        {
            character.SkillComponent.BombEntityId = null;
            return false;
        }

        IBattleEntitySkill bombSkill = bomb.Skills.FirstOrDefault();
        if (bombSkill == null)
        {
            character.SkillComponent.BombEntityId = null;
            return false;
        }

        character.CancelCastingSkill();
        SkillInfo fakeBombSkill = character.GetFakeBombSkill();
        fakeBombSkill.Cooldown = (short)(skill.Skill.Cooldown == 0 ? 0 : fakeBombSkill.Cooldown);
        character.Session.SendSkillCooldownResetAfter(fakeBombSkill.CastId, fakeBombSkill.Cooldown);

        bomb.EmitEvent(new BattleExecuteSkillEvent(bomb, bomb, bombSkill.Skill.GetInfo(), DateTime.UtcNow));
        character.SetSkillCooldown(fakeBombSkill);
        character.SkillComponent.BombEntityId = null;
        return true;
    }
}