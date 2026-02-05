using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.Battle;

public static class BattleEntitySharedExtensions
{
    public static async Task AddBuffAsync(this IBattleEntity battleEntity, params Buff[] buffs)
    {
        await battleEntity.EmitEventAsync(new BuffAddEvent(battleEntity, buffs));
    }

    public static async Task AddBuffsAsync(this IBattleEntity battleEntity, IEnumerable<Buff> buffs)
    {
        await battleEntity.EmitEventAsync(new BuffAddEvent(battleEntity, buffs));
    }

    public static async Task CheckAct4Buff(this IPlayerEntity playerEntity, IBuffFactory buffFactory)
    {
        if (playerEntity.MapInstance == null)
        {
            return;
        }

        if (playerEntity.BuffComponent == null)
        {
            return;
        }

        if (!playerEntity.MapInstance.HasMapFlag(MapFlags.HAS_IMMUNITY_ON_MAP_CHANGE_ENABLED))
        {
            return;
        }

        Buff buff = buffFactory.CreateBuff((short)BuffVnums.INVICIBLE_IN_PVP, playerEntity, BuffFlag.NORMAL);
        await playerEntity.AddBuffAsync(buff);

        if (!playerEntity.MateComponent.TeamMembers().Any())
        {
            return;
        }

        foreach (IMateEntity mate in playerEntity.MateComponent.TeamMembers())
        {
            await mate.AddBuffAsync(buff);
        }
    }

    public static async Task CheckPvPBuff(this IBattleEntity battleEntity)
    {
        if (battleEntity.MapInstance == null)
        {
            return;
        }

        if (battleEntity.BuffComponent == null)
        {
            return;
        }

        bool pvp = battleEntity.BuffComponent.HasBuff((int)BuffVnums.PVP);

        if (!pvp)
        {
            return;
        }

        if (!battleEntity.MapInstance.IsPvp)
        {
            return;
        }

        Buff buffFire = battleEntity.BuffComponent.GetBuff((int)BuffVnums.PVP);
        await battleEntity.RemoveBuffAsync(false, buffFire);
    }

    public static async Task CheckAct52Buff(this IPlayerEntity playerEntity, IBuffFactory buffFactory)
    {
        if (playerEntity.MapInstance == null)
        {
            return;
        }

        if (playerEntity.BuffComponent == null)
        {
            return;
        }

        if (playerEntity.MapInstance.HasMapFlag(MapFlags.HAS_BURNING_SWORD_ENABLED) == false)
        {
            if (!playerEntity.BuffComponent.HasBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF))
            {
                return;
            }

            Buff buff = playerEntity.BuffComponent.GetBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF);
            await playerEntity.RemoveBuffAsync(true, buff);
            foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.IsTeamMember))
            {
                await mate.RemoveBuffAsync(true, buff);
            }
            return;
        }

        bool oil = playerEntity.BuffComponent.HasBuff((int)BuffVnums.ICE_FLOWER);
        bool born = playerEntity.BuffComponent.HasBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF);

        if (born)
        {
            if (!oil)
            {
                return;
            }

            Buff buff = playerEntity.BuffComponent.GetBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF);
            await playerEntity.RemoveBuffAsync(true, buff);
            foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.IsTeamMember))
            {
                await mate.RemoveBuffAsync(true, buff);
            }
            return;
        }

        if (!oil)
        {
            Buff burnBuff = buffFactory.CreateBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF, playerEntity, BuffFlag.BIG | BuffFlag.NO_DURATION);
            await playerEntity.AddBuffAsync(burnBuff);
            foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.IsTeamMember))
            {
                await mate.AddBuffAsync(burnBuff);
            }
            return;
        }

        Buff buffFire = playerEntity.BuffComponent.GetBuff((int)BuffVnums.ACT_52_FIRE_DEBUFF);
        await playerEntity.RemoveBuffAsync(true, buffFire);
        foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.IsTeamMember))
        {
            await mate.RemoveBuffAsync(true, buffFire);
        }
    }

    public static async Task RemoveBuffAsync(this IBattleEntity battleEntity, int buffId, bool removePermanent = false)
    {
        Buff buff = battleEntity.BuffComponent.GetBuff(buffId);
        if (buff is null)
        {
            return;
        }

        await battleEntity.EmitEventAsync(new BuffRemoveEvent
        {
            Entity = battleEntity,
            Buffs = new[] { buff },
            RemovePermanentBuff = removePermanent
        });
    }

    public static async Task RemoveBuffAsync(this IBattleEntity battleEntity, bool removePermanentBuff, params Buff[] buffs)
    {
        if (buffs.Length == 0 || buffs.All(s => s == null))
        {
            return;
        }

        await battleEntity.EmitEventAsync(new BuffRemoveEvent
        {
            Entity = battleEntity,
            Buffs = buffs,
            RemovePermanentBuff = removePermanentBuff
        });
    }

    public static async Task RemoveAllBuffsAsync(this IBattleEntity entity, bool removePermanentBuff)
    {
        if (!entity.BuffComponent.HasAnyBuff())
        {
            return;
        }

        await entity.RemoveBuffAsync(removePermanentBuff, entity.BuffComponent.GetAllBuffs().ToArray());
    }

    public static async Task RemoveBuffsOnDeathAsync(this IBattleEntity entity)
    {
        if (!entity.BuffComponent.HasAnyBuff())
        {
            return;
        }

        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => !x.IsNotRemovedOnDeath());
        await entity.RemoveBuffAsync(false, buffs.ToArray());
    }

    public static async Task RemoveBuffsOnSpTransformAsync(this IBattleEntity entity)
    {
        if (!entity.BuffComponent.HasAnyBuff())
        {
            return;
        }

        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => !x.IsNotDisappearOnSpChange());
        await entity.RemoveBuffAsync(false, buffs.ToArray());
    }
}