using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;

namespace Plugin.Raids.Handlers;

public class RaidProcessChickenKingHitEventHandler : IAsyncEventProcessor<BattleExecuteSkillEvent>
{
    private readonly IRaidManager _raidManager;
    private readonly IRandomGenerator _randomGenerator;

    public RaidProcessChickenKingHitEventHandler(IRaidManager raidManager, IRandomGenerator randomGenerator)
    {
        _raidManager = raidManager;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(BattleExecuteSkillEvent e, CancellationToken cancellation)
    {
        IBattleEntity caster = e.Entity;
        if (!caster.IsMonster())
        {
            return;
        }

        if (!caster.IsAlive())
        {
            return;
        }

        if (caster is not IMonsterEntity monsterEntity)
        {
            return;
        }

        if (monsterEntity.MonsterVNum != (short)MonsterVnum.CHICKEN_KING)
        {
            return;
        }

        SkillInfo skill = e.SkillInfo;
        
        switch ((SkillsVnums)skill.Vnum)
        {
            case SkillsVnums.CHICKEN_KING_JUMP:
            case SkillsVnums.CHICKEN_KING_JUMP_QUICK:
            case SkillsVnums.CHICKEN_KING_JUMP_FAST:
                
                if (monsterEntity.IsJumping)
                {
                    return;
                }
        
                monsterEntity.IsJumping = true;

                List<string> packets = new();
                IReadOnlyList<IBattleEntity> enemies = monsterEntity.GetEnemiesInRange(monsterEntity, skill.Range).ToList();

                if (enemies.Count == 0)
                {
                    return;
                }
                
                foreach (IBattleEntity entity in enemies)
                {
                    packets.Add(entity.GenerateEffectGround(EffectType.RedCircle, entity.Position.X, entity.Position.Y, false));
                }
                
                monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateUntarget());
                monsterEntity.MapInstance.Broadcast(packets);
                
                RaidParty raidParty = _raidManager.GetRaidPartyByMapInstanceId(caster.MapInstance.Id);
                if (raidParty?.Instance?.RaidSubInstances == null)
                {
                    return;
                }

                if (raidParty.Finished)
                {
                    return;
                }

                if (!raidParty.Instance.RaidSubInstances.TryGetValue(caster.MapInstance.Id, out RaidSubInstance raidSubInstance) || raidSubInstance?.MapInstance == null)
                {
                    return;
                }

                raidSubInstance.SavedTargetPosition = enemies.ElementAt(_randomGenerator.RandomNumber(enemies.Count)).Position;

                break;
        }
    }
}