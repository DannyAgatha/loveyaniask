using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Act4.Const;
using Plugin.Act4.Extension;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Monster.Event;

namespace Plugin.Act4.RecurrentJob;

public class Act4DungeonSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IBuffFactory _buffFactory;
    private readonly IDungeonManager _dungeonManager;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;

    public Act4DungeonSystem(IAct4DungeonManager act4dungeonManager, IAsyncEventPipeline asyncEventPipeline,
        IDungeonManager dungeonManager, GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory)
    {
        _act4DungeonManager = act4dungeonManager;
        _asyncEventPipeline = asyncEventPipeline;
        _dungeonManager = dungeonManager;
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffFactory = buffFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[ACT4_DUNGEON_SYSTEM] Started!");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Process(stoppingToken);
            }
            catch (Exception e)
            {
                Log.Error("[ACT4_DUNGEON_SYSTEM]", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task Process(CancellationToken stoppingToken)
    {
        if (!_act4DungeonManager.DungeonsActive)
        {
            return;
        }

        DateTime currentTime = DateTime.UtcNow;

        foreach (DungeonInstance dungeon in _act4DungeonManager.Dungeons.ToArray())
        {
            try
            {
                await ProcessAct4DungeonInstanceAfterSlowMo(dungeon, currentTime);
                await ProcessAct4DungeonInstanceCleanUp(dungeon, currentTime);

                foreach (DungeonSubInstance subInstance in dungeon.DungeonSubInstances.Values)
                {
                    try
                    {
                        await ProcessAct4HatusInstance(subInstance, currentTime);
                        await ProcessAct4DungeonCalvinas(subInstance, currentTime, dungeon.DungeonType);
                        await ProcessAct4DungeonSubInstanceLoopWave(subInstance, currentTime, stoppingToken);
                        await ProcessAct4DungeonSubInstanceWave(subInstance, currentTime, stoppingToken);
                        await ProcessAct4DungeonSubInstancePortalGeneration(dungeon, subInstance, currentTime, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[DUNGEON_SUBINSTANCE_PROCESS] familyId: {dungeon.FamilyId}", e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"[DUNGEON_PROCESS] familyId: {dungeon.FamilyId}", e);
            }
        }

        if (_act4DungeonManager.DungeonEnd < currentTime)
        {
            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonSystemStopEvent(), stoppingToken);
        }
    }

    private async Task ProcessAct4DungeonCalvinas(DungeonSubInstance dungeon, DateTime currentTime, DungeonType dungeonType)
    {
        if (dungeonType != DungeonType.Calvinas)
        {
            return;
        }

        CalvinasState calvinasState = _dungeonManager.GetCalvinasDragons(dungeon.MapInstance.Id);
        if (calvinasState == null)
        {
            return;
        }

        if (calvinasState.CastTime > currentTime)
        {
            return;
        }

        _dungeonManager.RemoveCalvinasDragons(dungeon.MapInstance.Id);

        if (calvinasState.CalvinasDragonsList == null)
        {
            return;
        }

        IEnumerable<IBattleEntity> entities = dungeon.MapInstance.GetNonMonsterBattleEntities();

        IMonsterEntity boss = dungeon.Bosses.FirstOrDefault();
        foreach (IBattleEntity entity in entities)
        {
            // checks if the player/mate is inside the (dragon) rectangle
            foreach (CalvinasDragon dragon in calvinasState.CalvinasDragonsList)
            {
                bool isInX = false;
                bool isInY = false;
                if (dragon.Axis == CoordType.X)
                {
                    isInY = entity.IsInLineY(dragon.At, dragon.Size);

                    if (dragon.Start < dragon.End)
                    {
                        if (dragon.Start <= entity.PositionX && entity.PositionX <= dragon.End)
                        {
                            isInX = true;
                        }
                    }
                    else
                    {
                        if (dragon.End <= entity.PositionX && entity.PositionX <= dragon.Start)
                        {
                            isInX = true;
                        }
                    }
                }
                else
                {
                    isInX = entity.IsInLineX(dragon.At, dragon.Size);

                    if (dragon.Start < dragon.End)
                    {
                        if (dragon.Start <= entity.PositionY && entity.PositionY <= dragon.End)
                        {
                            isInY = true;
                        }
                    }
                    else
                    {
                        if (dragon.End <= entity.PositionY && entity.PositionY <= dragon.Start)
                        {
                            isInY = true;
                        }
                    }
                }

                if (!isInX || !isInY)
                {
                    continue;
                }

                if (!entity.IsAlive())
                {
                    continue;
                }

                if (await boss.ShouldSaveDefender(entity, entity.MaxHp))
                {
                    continue;
                }

                entity.Hp = 0;
                await entity.EmitEventAsync(new GenerateEntityDeathEvent
                {
                    Entity = entity
                });
                boss.BroadcastCleanSuPacket(entity, entity.MaxHp);
                break;
            }
        }
    }

    private async Task ProcessAct4HatusInstance(DungeonSubInstance dungeon, DateTime currentTime)
    {
        try
        {
            if (dungeon?.HatusHeads?.DragonHeads == null)
            {
                return;
            }

            if (dungeon.Bosses.Count == 0)
            {
                return;
            }

            HatusState hatusState = _dungeonManager.GetHatusState(dungeon.MapInstance.Id);
            if (hatusState == null)
            {
                return;
            }

            HatusHeads heads = dungeon.HatusHeads;

            if (heads.HeadsState == HatusDragonHeadState.HIDE_HEAD)
            {
                return;
            }

            short headsChange = 0;

            HatusDragonHeadState toChange = heads.HeadsState;
            short[] xToHit = new short[3];

            switch (heads.HeadsState)
            {
                case HatusDragonHeadState.SHOW:
                case HatusDragonHeadState.IDLE:

                    if (hatusState.BlueAttack)
                    {
                        headsChange += 1;
                        heads.DragonHeads.BluePositionX = hatusState.BlueX;
                        heads.DragonHeads.BlueIsActive = true;
                        toChange = HatusDragonHeadState.ATTACK_CAST;
                    }

                    if (hatusState.RedAttack)
                    {
                        headsChange += 2;
                        heads.DragonHeads.RedPositionX = hatusState.RedX;
                        heads.DragonHeads.RedIsActive = true;
                        toChange = HatusDragonHeadState.ATTACK_CAST;
                    }

                    if (hatusState.GreenAttack)
                    {
                        headsChange += 4;
                        heads.DragonHeads.GreenPositionX = hatusState.GreenX;
                        heads.DragonHeads.GreenIsActive = true;
                        toChange = HatusDragonHeadState.ATTACK_CAST;
                    }

                    heads.CastTime = currentTime + hatusState.CastTime;

                    break;
                case HatusDragonHeadState.ATTACK_CAST:

                    if (heads.CastTime < currentTime)
                    {
                        toChange = HatusDragonHeadState.ATTACK_USE;

                        if (heads.DragonHeads.BlueIsActive)
                        {
                            headsChange += 1;
                        }

                        if (heads.DragonHeads.RedIsActive)
                        {
                            headsChange += 2;
                        }

                        if (heads.DragonHeads.GreenIsActive)
                        {
                            headsChange += 4;
                        }

                        heads.CastTime = currentTime + hatusState.CastTime;
                    }

                    break;
                case HatusDragonHeadState.ATTACK_USE:

                    if (heads.CastTime > currentTime)
                    {
                        break;
                    }

                    if (heads.DragonHeads.BlueIsActive)
                    {
                        headsChange += 1;
                        heads.DragonHeads.BlueIsActive = false;
                        xToHit[0] = heads.DragonHeads.BluePositionX;
                    }

                    if (heads.DragonHeads.RedIsActive)
                    {
                        headsChange += 2;
                        heads.DragonHeads.RedIsActive = false;
                        xToHit[1] = heads.DragonHeads.RedPositionX;
                    }

                    if (heads.DragonHeads.GreenIsActive)
                    {
                        headsChange += 4;
                        heads.DragonHeads.GreenIsActive = false;
                        xToHit[2] = heads.DragonHeads.GreenPositionX;
                    }

                    break;
            }

            if (headsChange == 0)
            {
                if (heads.CastTime > currentTime)
                {
                    return;
                }

                _dungeonManager.RemoveHatusState(dungeon.MapInstance.Id);
                heads.HeadsState = HatusDragonHeadState.IDLE;
                return;
            }

            dungeon.MapInstance.Broadcast(Act4DungeonExtension.HatusHeadStatePacket(headsChange, heads));
            heads.HeadsState = toChange;

            if (heads.HeadsState == HatusDragonHeadState.ATTACK_USE)
            {
                if (heads.CastTime > currentTime)
                {
                    return;
                }

                List<IBattleEntity> entitesToAttack = new();
                IMonsterEntity boss = dungeon.Bosses.FirstOrDefault();

                foreach (short x in xToHit)
                {
                    entitesToAttack.AddRange(dungeon.MapInstance.GetNonMonsterBattleEntities().Where(e => e.IsInLineX(x, (short)heads.DragonAttackWidth)));
                }

                foreach (IBattleEntity entity in entitesToAttack)
                {
                    if (!entity.IsAlive())
                    {
                        continue;
                    }

                    int damage = (int)(entity.MaxHp * hatusState.DealtDamage);
                    if (await boss.ShouldSaveDefender(entity, damage))
                    {
                        continue;
                    }

                    if (entity.Hp - damage <= 0)
                    {
                        entity.Hp = 0;
                        await entity.EmitEventAsync(new GenerateEntityDeathEvent
                        {
                            Entity = entity
                        });

                        boss.BroadcastCleanSuPacket(entity, damage);
                        continue;
                    }

                    entity.Hp -= damage;

                    switch (entity)
                    {
                        case IPlayerEntity character:
                            character.LastDefence = DateTime.UtcNow;
                            character.Session.RefreshStat();

                            if (character.IsSitting)
                            {
                                await character.Session.RestAsync(force: true);
                            }

                            break;
                        case IMateEntity mate:
                            mate.LastDefence = DateTime.UtcNow;
                            mate.Owner.Session.SendMateLife(mate);

                            if (mate.IsSitting)
                            {
                                await mate.Owner.Session.EmitEventAsync(new MateRestEvent
                                {
                                    MateEntity = mate,
                                    Force = true
                                });
                            }

                            break;
                    }

                    boss.BroadcastCleanSuPacket(entity, damage);
                }

                heads.HeadsState = HatusDragonHeadState.IDLE;
                _dungeonManager.RemoveHatusState(dungeon.MapInstance.Id);
            }
        }
        catch (Exception e)
        {
            Log.Error("[PROCESS_HATUS]", e);
        }
    }

    private async Task ProcessAct4DungeonSubInstanceLoopWave(DungeonSubInstance dungeonSubInstance, DateTime currentTime, CancellationToken stoppingToken)
    {
        try
        {
            if (dungeonSubInstance.LastDungeonWaveLoop == null || dungeonSubInstance.LoopWaves.Count < 1)
            {
                return;
            }

            foreach (DungeonLoopWave wave in dungeonSubInstance.LoopWaves)
            {
                if (wave.FirstSpawnWave > currentTime)
                {
                    continue;
                }

                if (wave.LastMonsterSpawn > currentTime)
                {
                    continue;
                }

                wave.LastMonsterSpawn = currentTime + wave.TickDelay;
                short? scaledWithPlayersAmount = wave.IsScaledWithPlayerAmount ? (short?)dungeonSubInstance.MapInstance.Sessions.Count : null;
                await _asyncEventPipeline.ProcessEventAsync(new MonsterSummonEvent(dungeonSubInstance.MapInstance, wave.Monsters, scaledWithPlayerAmount: scaledWithPlayersAmount), stoppingToken);
            }
        }
        catch (Exception e)
        {
            Log.Error("[PROCESS_DUNGEON_LOOP]", e);
        }
    }

    private async Task ProcessAct4DungeonSubInstanceWave(DungeonSubInstance dungeonSubInstance, DateTime currentTime, CancellationToken stoppingToken)
    {
        try
        {
            DungeonLinearWave wave = dungeonSubInstance.LinearWaves.FirstOrDefault();
            if (wave == null || dungeonSubInstance.LastDungeonWaveLinear == null || currentTime < dungeonSubInstance.LastDungeonWaveLinear + wave.Delay)
            {
                return;
            }

            dungeonSubInstance.LinearWaves.Remove(wave);
            await _asyncEventPipeline.ProcessEventAsync(new MonsterSummonEvent(dungeonSubInstance.MapInstance, wave.Monsters), stoppingToken);
        }
        catch (Exception e)
        {
            Log.Error("[PROCESS_DUNGEON_WAVE]", e);
        }
    }

    private async Task ProcessAct4DungeonSubInstancePortalGeneration(DungeonInstance dungeonInstance, DungeonSubInstance dungeonSubInstance, DateTime currentTime, CancellationToken stoppingToken)
    {
        PortalGenerator portalGenerator = dungeonSubInstance.PortalGenerators.FirstOrDefault();
        if (portalGenerator == null || dungeonSubInstance.LastPortalGeneration == null || currentTime < _act4DungeonManager.DungeonStart + portalGenerator.Delay)
        {
            return;
        }

        dungeonSubInstance.PortalGenerators.Remove(portalGenerator);

        await _asyncEventPipeline.ProcessEventAsync(new SpawnPortalEvent(dungeonSubInstance.MapInstance, portalGenerator.Portal), stoppingToken);
        //quick win
        await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonBroadcastPacketEvent
        {
            DungeonInstance = dungeonInstance
        }, stoppingToken);
        await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonBroadcastBossOpenEvent
        {
            DungeonInstance = dungeonInstance
        }, stoppingToken);
    }

    private static async Task ProcessAct4DungeonInstanceAfterSlowMo(DungeonInstance dungeonInstance, DateTime currentTime)
    {
        if (dungeonInstance.FinishSlowMoDate == null || currentTime < dungeonInstance.FinishSlowMoDate)
        {
            return;
        }

        dungeonInstance.FinishSlowMoDate = DateTime.MaxValue;
        foreach (DungeonSubInstance subInstance in dungeonInstance.DungeonSubInstances.Values)
        {
            await subInstance.TriggerEvents(DungeonConstEventKeys.RaidSubInstanceAfterSlowMo);
        }
    }

    private static async Task ProcessAct4DungeonInstanceCleanUp(DungeonInstance dungeonInstance, DateTime currentTime)
    {
        if (dungeonInstance.CleanUpBossMapDate == null || currentTime < dungeonInstance.CleanUpBossMapDate)
        {
            return;
        }

        dungeonInstance.CleanUpBossMapDate = null;
        foreach (DungeonSubInstance subInstance in dungeonInstance.DungeonSubInstances.Values)
        {
            await subInstance.TriggerEvents(DungeonConstEventKeys.RaidSubInstanceCleanUp);
        }
    }
}