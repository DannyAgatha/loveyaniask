using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Triggers;

namespace NosEmu.Plugins.BasicImplementations.Event.Monster;

public class MonsterSummonEventHandler : IAsyncEventProcessor<MonsterSummonEvent>
{
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IPhantomPositionManager _phantomPositionManager;
    private readonly IRandomGenerator _randomGenerator;
    
    public MonsterSummonEventHandler(IRandomGenerator randomGenerator, INpcMonsterManager npcMonsterManager,
        IMonsterEntityFactory monsterEntityFactory, IPhantomPositionManager phantomPositionManager)
    {
        _randomGenerator = randomGenerator;
        _npcMonsterManager = npcMonsterManager;
        _monsterEntityFactory = monsterEntityFactory;
        _phantomPositionManager = phantomPositionManager;
    }

    public async Task HandleAsync(MonsterSummonEvent e, CancellationToken cancellation)
    {
        List<ToSummon> toSummon = new();
        short? scaledAmount = e.ScaledWithPlayerAmount;

        if (scaledAmount.HasValue)
        {
            for (int i = 0; i < scaledAmount.Value; i++)
            {
                toSummon.AddRange(e.Monsters);
            }
        }
        else
        {
            toSummon = e.Monsters.ToList();
        }

        foreach (ToSummon summon in toSummon)
        {
            IMonsterData npcMonster = _npcMonsterManager.GetNpc(summon.VNum);
            if (npcMonster == null || _randomGenerator.RandomNumber() > summon.SummonChance)
            {
                continue;
            }

            if (e.Map.IsSummonLimitReached(e.Summoner?.Id, summon.SummonType))
            {
                continue;
            }
            
            if (summon.SpawnCell == null)
            {
                continue;
            }
            
            IMonsterEntity mapMonster = summon.MonsterEntity ?? _monsterEntityFactory.CreateMonster(npcMonster, e.Map, new MonsterEntityBuilder
            {
                IsMateTrainer = summon.IsMateTrainer,
                IsBonus = summon.IsBonusOrProtected,
                IsBoss = summon.IsBossOrMate,
                IsTarget = summon.IsTarget,
                IsHostile = summon.IsHostile,
                IsSparringMonster = summon.IsSparringMonster,
                IsWalkingAround = summon.IsMoving,
                IsVesselMonster = summon.IsVesselMonster,
                IsVesselChristmasMonster = summon.IsVesselChristmasMonster,
                SummonType = summon.SummonType,
                SummonerId = e.Summoner?.Id,
                SummonerType = e.Summoner?.Type,
                FactionType = summon.FactionType ?? e.Summoner?.Faction,
                HpMultiplier = summon.HpMultiplier,
                MpMultiplier = summon.MpMultiplier,
                SetHitChance = summon.SetHitChance != 0 && !summon.IsBossOrMate ? summon.SetHitChance : npcMonster.BasicHitChance,
                Direction = summon.Direction,
                GoToBossPosition = summon.GoToBossPosition,
                PositionX = summon.SpawnCell.Value.X,
                PositionY = summon.SpawnCell.Value.Y,
                IsInstantBattle = summon.IsInstantBattle,
                Level = summon.Level,
                IgnoreSkillRange = summon.IgnoreSkillRange,
                IsTotem = summon.IsTotem,
                EffectId = summon.EffectId
            });

            mapMonster.Target = summon.Target;
            mapMonster.Waypoints = summon.Waypoints;

            if (summon.TriggerEvents != null)
            {
                foreach ((string key, IAsyncEvent asyncEvent, bool removeOnUse) in summon.TriggerEvents)
                {
                    mapMonster.AddEvent(key, asyncEvent, removeOnUse);
                }
            }

            if (e.Summoner != null)
            {
                if (e.GetSummonerLevel)
                {
                    mapMonster.Level = e.Summoner.Level;
                }

                if (e.Summoner is IPlayerEntity player && summon.VNum == (short)MonsterVnum.BOMB)
                {
                    player.SkillComponent.BombEntityId = mapMonster.Id;
                }
            }

            Position? phantomPosition = e.NpcId.HasValue ? _phantomPositionManager.GetPosition(e.NpcId.Value) : null;
            Position spawnCell = phantomPosition ?? summon.SpawnCell ?? e.Map.GetRandomPosition();

            if (summon.AtAroundMobId.HasValue && summon.AtAroundMobRange.HasValue)
            {
                IMonsterEntity originalEntity = e.Map.GetMonsterByUniqueId(summon.AtAroundMobId.Value);
                byte range = summon.AtAroundMobRange.Value;
                if (originalEntity is not null)
                {
                    spawnCell = new Position((short)(originalEntity.PositionX + _randomGenerator.RandomNumber(-range, range)),
                        (short)(originalEntity.PositionY + _randomGenerator.RandomNumber(-range, range)));

                    if (originalEntity.MapInstance.IsBlockedZone(spawnCell.X, spawnCell.Y))
                    {
                        spawnCell = originalEntity.Position;
                    }
                }
            }


            await mapMonster.EmitEventAsync(new MapJoinMonsterEntityEvent(mapMonster, spawnCell.X, spawnCell.Y, e.ShowEffect));

            if (summon.EffectId.HasValue && summon.SpawnCell.HasValue)
            {
                short effectId = summon.EffectId.Value;
                Position spawnPosition = summon.SpawnCell.Value;

                await mapMonster.MapInstance.BroadcastAsync(_ => 
                    Task.FromResult(mapMonster.GenerateEffectGround(
                        (EffectType)effectId, 
                        spawnPosition.X, 
                        spawnPosition.Y, 
                        false
                    )));
            }
            
            CheckBeriosPhantom(mapMonster);

            DateTime now = DateTime.UtcNow;

            if (summon.IncreaseTick)
            {
                mapMonster.NextTick = now;
                mapMonster.NextAttackReady = now;
                mapMonster.NextTick += TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(2000));
                mapMonster.NextAttackReady += TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(2000));
            }

            mapMonster.AttentionTime = now + TimeSpan.FromSeconds(10);
            if (!summon.RemoveTick)
            {
                continue;
            }

            mapMonster.NextTick = DateTime.UtcNow.AddSeconds(-1000);
            mapMonster.NextAttackReady = DateTime.UtcNow.AddSeconds(-1000);
        }
    }

    private void CheckBeriosPhantom(IMonsterEntity mapMonster)
    {
        switch ((MonsterVnum)mapMonster.MonsterVNum)
        {
            case MonsterVnum.EMERALD_PHANTOM:

                mapMonster.AddEvent(BattleTriggers.OnDeath, new NpcSummonEvent
                {
                    Map = mapMonster.MapInstance,
                    MonsterId = mapMonster.UniqueId,
                    Npcs = Lists.Create(new ToSummon
                    {
                        VNum = (short)MonsterVnum.EMERALD_SHADOW_PHANTOM,
                        SpawnCell = mapMonster.Position,
                        IsMoving = true,
                        IsHostile = true
                    })
                }, true);

                break;

            case MonsterVnum.SAPPHIRE_PHANTOM:

                mapMonster.AddEvent(BattleTriggers.OnDeath, new NpcSummonEvent
                {
                    Map = mapMonster.MapInstance,
                    MonsterId = mapMonster.UniqueId,
                    Npcs = Lists.Create(new ToSummon
                    {
                        VNum = (short)MonsterVnum.SAPPHIRE_SHADOW_PHANTOM,
                        SpawnCell = mapMonster.Position,
                        IsMoving = true,
                        IsHostile = true
                    })
                }, true);

                break;

            case MonsterVnum.RUBY_PHANTOM:

                mapMonster.AddEvent(BattleTriggers.OnDeath, new NpcSummonEvent
                {
                    Map = mapMonster.MapInstance,
                    MonsterId = mapMonster.UniqueId,
                    Npcs = Lists.Create(new ToSummon
                    {
                        VNum = (short)MonsterVnum.RUBY_SHADOW_PHANTOM,
                        SpawnCell = mapMonster.Position,
                        IsMoving = true,
                        IsHostile = true
                    })
                }, true);

                break;

            default:
                return;
        }
    }
}