using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.Raids.Handlers;

public class RaidProcessLordDracoJumpEventHandler : IAsyncEventProcessor<RaidProcessBossMechanicsEvent>
{
    private readonly IRaidManager _raidManager;

    public RaidProcessLordDracoJumpEventHandler(IRaidManager raidManager)
    {
        _raidManager = raidManager;
    }

    public async Task HandleAsync(RaidProcessBossMechanicsEvent e, CancellationToken cancellation)
    {
        IBattleEntity lordDraco = e.BattleEntity;
        SkillInfo skill = e.SkillInfo;

        if (skill.Vnum != (short)SkillsVnums.DRAGON_JUMP)
        {
            return;
        }

        if (lordDraco is not IMonsterEntity monsterEntity)
        {
            return;
        }

        if (monsterEntity.MonsterVNum != (short)MonsterVnum.LORD_DRACO)
        {
            return;
        }

        if (!monsterEntity.IsAlive())
        {
            return;
        }

        if (!monsterEntity.IsJumping)
        {
            return;
        }

        monsterEntity.IsJumping = false;
        RaidParty raidParty = _raidManager.GetRaidPartyByMapInstanceId(monsterEntity.MapInstance.Id);
        if (raidParty?.Instance?.RaidSubInstances == null)
        {
            return;
        }

        if (raidParty.Finished)
        {
            return;
        }

        if (!raidParty.Instance.RaidSubInstances.TryGetValue(monsterEntity.MapInstance.Id, out RaidSubInstance raidSubInstance) || raidSubInstance?.MapInstance == null)
        {
            return;
        }

        if (raidSubInstance.SavedTargetPosition == null || raidSubInstance.SavedTargetPosition.Value == default)
        {
            return;
        }

        Position position = raidSubInstance.SavedTargetPosition.Value;
        monsterEntity.TeleportOnMap(position.X, position.Y);

        monsterEntity.SkillToUse = (short)SkillsVnums.DRAGON_STAGGER;
        monsterEntity.ForceUseSkill = true;

        foreach (IClientSession session in monsterEntity.MapInstance.Sessions)
        {
            session.SendSound(monsterEntity, SoundType.LORD_DRACO_LAND);
        }

        IEnumerable<IBattleEntity> enemiesInRange = monsterEntity.GetEnemiesInRange(monsterEntity, 2);

        foreach (IBattleEntity entity in enemiesInRange)
        {
            if (!entity.IsAlive())
            {
                continue;
            }

            if (await monsterEntity.ShouldSaveDefender(entity, entity.MaxHp))
            {
                continue;
            }

            if (entity.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath) ||
                entity.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoDeath) ||
                    entity.BCardComponent.HasBCard(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.NoHPConsumption) ||
                        entity.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
            {
                continue;
            }
            
            entity.Hp = 0;
            await entity.EmitEventAsync(new GenerateEntityDeathEvent
            {
                Entity = entity,
                Attacker = monsterEntity
            });

            monsterEntity.BroadcastCleanSuPacket(entity, entity.MaxHp);
        }
    }
}