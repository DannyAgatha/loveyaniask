using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.CoreImpl.Pathfinding;
using WingsAPI.Data.Fish;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.CoreImpl.Maps.Systems
{
    public class MonsterSystem : IMapSystem, IMonsterSystem
    {
        private const int TICK_DELAY_MILLISECONDS = 50;
        private const int AGGRO_LIMIT_PER_ENTITY = 100;
        private const int RETURN_TIME_OUT = 60;
        private static DateTime lastSpawnTime = DateTime.MinValue;

        private const int SUMMONED_MONSTERS_LIMIT_PER_ENTITY = 40;
        private const int SUMMONED_MONSTERS_LIMIT_MONSTER_WAVES = 120;

        private static readonly TimeSpan _refreshRate = TimeSpan.FromMilliseconds(TICK_DELAY_MILLISECONDS);

        private readonly Dictionary<int, Dictionary<long, long>> _attackerByRace = new();
        private readonly IBCardEffectHandlerContainer _bcardHandlers;
        private readonly IBuffFactory _buff;
        private readonly BCardTickSystem _bCardTickSystem;
        private readonly IAsyncEventPipeline _eventPipeline;

        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly IMapInstance _mapInstance;
        private readonly List<IMonsterEntity> _monsters = [];
        private readonly ConcurrentDictionary<long, IMonsterEntity> _monstersById = new();
        private readonly ConcurrentDictionary<Guid, IMonsterEntity> _monstersByUniqueId = new();
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IMonsterTalkingConfig _monsterTalkingConfig;
        private readonly IPathFinder _pathFinder;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ITrainerSpecialistConfiguration _trainerSpecialistConfiguration;
        private readonly PetMaxLevelConfiguration _petMaxLevelConfiguration;
        private readonly ConcurrentDictionary<long, int> _summonedByMonsterId = new();
        private readonly ConcurrentQueue<IMonsterEntity> _toAddMonsters = new();
        private readonly ConcurrentQueue<IMonsterEntity> _toRemoveMonsters = new();
        private byte _currentVessels;
        private DateTime _lastProcess = DateTime.MinValue;
        private int _summonMonsterWaveCounter;
        private long _totalMonstersDeaths;
        private readonly IFishManager _fishManager;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    // Belial
        private DateTime _lastCheckTime = DateTime.MinValue;
        private readonly Dictionary<IMonsterEntity, DateTime> _totemSpawnTime = new();
        private readonly HashSet<IPlayerEntity> _playersInBlackTotemZone = [];
        private readonly HashSet<IPlayerEntity> _playersAlreadyDamaged = [];
        private const int BLACK_TOTEM_DURATION = 9000;
        private const int WHITE_TOTEM_DURATION = 4000;

        public MonsterSystem(IRandomGenerator randomGenerator, IBCardEffectHandlerContainer bcardHandlers, IAsyncEventPipeline eventPipeline, IMapInstance mapInstance, IBuffFactory buffFactory,
            IGameLanguageService gameLanguage, IPathFinder pathFinder, IMonsterTalkingConfig monsterTalkingConfig,
            IFishManager fishManager, ITrainerSpecialistConfiguration trainerSpecialistConfiguration, PetMaxLevelConfiguration petMaxLevelConfiguration,
            RainbowBattleConfiguration rainbowBattleConfiguration)
        {
            _randomGenerator = randomGenerator;
            _bcardHandlers = bcardHandlers;
            _eventPipeline = eventPipeline;
            _mapInstance = mapInstance;
            _bCardTickSystem = new BCardTickSystem(bcardHandlers, _randomGenerator, buffFactory, gameLanguage);
            _pathFinder = pathFinder;
            _monsterTalkingConfig = monsterTalkingConfig;
            _trainerSpecialistConfiguration = trainerSpecialistConfiguration;
            _petMaxLevelConfiguration = petMaxLevelConfiguration;
            _fishManager = fishManager;
            _totalMonstersDeaths = 0;
            _buff = buffFactory;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _gameLanguageService = gameLanguage;
        }


        public void PutIdleState()
        {
            _bCardTickSystem.Clear();
            _attackerByRace.Clear();
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _monsters.Clear();
                _monstersById.Clear();
                _attackerByRace.Clear();
                _toAddMonsters.Clear();
                _toRemoveMonsters.Clear();
                _monstersByUniqueId.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public string Name => nameof(MonsterSystem);

        public void ProcessTick(DateTime date, bool isTickRefresh = false)
        {
            if (_lastProcess + _refreshRate > date)
            {
                return;
            }

            _lastProcess = date;

            _lock.EnterWriteLock();
            try
            {
                while (_toRemoveMonsters.TryDequeue(out IMonsterEntity toRemove))
                {
                    if (toRemove.Target != null)
                    {
                        RemoveTarget(toRemove, toRemove);
                    }

                    _monstersById.TryRemove(toRemove.Id, out _);
                    _monstersByUniqueId.TryRemove(toRemove.UniqueId, out _);
                    _monsters.Remove(toRemove);

                    if (toRemove.VesselMonster || toRemove.VesselChristmasMonster)
                    {
                        _currentVessels--;
                    }

                    switch (toRemove.SummonType)
                    {
                        case SummonType.MONSTER_WAVE:
                            _summonMonsterWaveCounter--;
                            break;
                    }

                    if (!toRemove.SummonerId.HasValue || toRemove.SummonerType is not VisualType.Monster)
                    {
                        continue;
                    }

                    if (_summonedByMonsterId.TryGetValue(toRemove.SummonerId.Value, out int currentMonsters))
                    {
                        _summonedByMonsterId.TryUpdate(toRemove.SummonerId.Value, currentMonsters - 1, currentMonsters);
                    }
                }

                while (_toAddMonsters.TryDequeue(out IMonsterEntity entity))
                {
                    if (entity.VesselMonster || entity.VesselChristmasMonster)
                    {
                        _currentVessels++;
                    }

                    _monstersById[entity.Id] = entity;
                    _monsters.Add(entity);
                    _monstersByUniqueId.TryAdd(entity.UniqueId, entity);

                    switch (entity.SummonType)
                    {
                        case SummonType.MONSTER_WAVE:
                            _summonMonsterWaveCounter++;
                            break;
                    }

                    if (!entity.SummonerId.HasValue || entity.SummonerType is not VisualType.Monster)
                    {
                        continue;
                    }

                    if (!_summonedByMonsterId.TryGetValue(entity.SummonerId.Value, out int currentMonsters))
                    {
                        currentMonsters = 1;
                        _summonedByMonsterId.TryAdd(entity.SummonerId.Value, currentMonsters);
                    }
                    else
                    {
                        _summonedByMonsterId.TryUpdate(entity.SummonerId.Value, currentMonsters + 1, currentMonsters);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            var activeMonsters = _monsters.ToList();
            foreach (IMonsterEntity monster in activeMonsters)
            {
                Update(date, monster, isTickRefresh);
            }
        }

        public void ActivateMode(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.EventGameInstance)
            {
                return;
            }

            monsterEntity.ModeIsActive = true;

            if (monsterEntity.ModeCModeVnum != 0)
            {
                monsterEntity.Morph = monsterEntity.ModeCModeVnum;
                monsterEntity.BroadcastMonsterMorph();
            }

            AddModeBCards(monsterEntity);
        }

        public void DeactivateMode(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.EventGameInstance)
            {
                return;
            }

            if (!monsterEntity.HasMode)
            {
                return;
            }

            monsterEntity.ModeIsActive = false;

            if (monsterEntity.Morph != 0)
            {
                monsterEntity.Morph = 0;
                monsterEntity.BroadcastMonsterMorph(-1);
            }

            RemoveModeBCards(monsterEntity);
        }

        public void IncreaseMonsterDeathsOnMap()
        {
            _totalMonstersDeaths++;
        }

        public long MonsterDeathsOnMap() => _totalMonstersDeaths;
        public byte CurrentVessels() => _currentVessels;

        public bool IsSummonLimitReached(int? summonerId, SummonType? summonSummonType)
        {
            if (summonSummonType is SummonType.MONSTER_WAVE)
            {
                return _summonMonsterWaveCounter >= SUMMONED_MONSTERS_LIMIT_MONSTER_WAVES;
            }

            if (summonerId is null)
            {
                return false;
            }

            if (!_summonedByMonsterId.TryGetValue(summonerId.Value, out int summonedMonsters))
            {
                return false;
            }

            return summonedMonsters >= SUMMONED_MONSTERS_LIMIT_PER_ENTITY;
        }

        public void RemoveTarget(IMonsterEntity monsterEntity, IBattleEntity entityToRemove, bool checkIfPlayer = false)
        {
            if (checkIfPlayer)
            {
                IPlayerEntity playerEntity = entityToRemove switch
                {
                    IPlayerEntity player => player,
                    IMateEntity mateEntity => mateEntity?.Owner,
                    _ => null
                };

                if (playerEntity != null)
                {
                    AggroIsRemoved(monsterEntity, playerEntity);
                    foreach (IBattleEntity mate in monsterEntity.Targets.Where(x => x is IMateEntity mateEntity && mateEntity.Owner?.Id == playerEntity.Id))
                    {
                        AggroIsRemoved(monsterEntity, mate);
                    }
                }
                else
                {
                    AggroIsRemoved(monsterEntity, entityToRemove);
                }
            }
            else
            {
                AggroIsRemoved(monsterEntity, entityToRemove);
            }

            if (monsterEntity.GroupAttack != (int)GroupAttackType.None)
            {
                RemoveTargetFromGroupAttackers(monsterEntity, entityToRemove);
            }

            monsterEntity.Targets.Remove(entityToRemove);

            if (monsterEntity.Target != null && monsterEntity.Target.IsSameEntity(entityToRemove))
            {
                monsterEntity.Target = null;
            }

            TryApproachToClosestTarget(monsterEntity);
            monsterEntity.IsApproachingTarget = false;
            if (entityToRemove != null)
            {
                monsterEntity.PlayersDamage.TryRemove(entityToRemove.Id, out _);
            }

            monsterEntity.ShouldFindNewTarget = monsterEntity.Targets.Count == 0;
        }

        public void AddEntityToTargets(IMonsterEntity monsterEntity, IBattleEntity target)
        {
            if (target == null)
            {
                return;
            }

            if (!monsterEntity.CanSeeInvisible && target.IsInvisible())
            {
                return;
            }

            if (!AddPlayerOrMateTarget(monsterEntity, target, false))
            {
                return;
            }

            monsterEntity.NextTick -= TimeSpan.FromMilliseconds(800);
            monsterEntity.AttentionTime = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            if (monsterEntity.GroupAttack != (short)GroupAttackType.None)
            {
                AddAttackerToGroupAttackers(monsterEntity, target);
            }

            IsAggroLimitReached(monsterEntity, target, true, true);
            TryApproachToClosestTarget(monsterEntity);
        }

        public void MonsterRefreshTarget(IMonsterEntity monsterEntity, IBattleEntity target, in DateTime time, bool isByAttacking = false)
        {
            monsterEntity.ReturnTimeOut = 0;
            if (monsterEntity.Targets.Contains(target))
            {
                if (!isByAttacking)
                {
                    return;
                }

                AddPlayerOrMateTarget(monsterEntity, target, true);
                monsterEntity.AttentionTime = time + TimeSpan.FromMinutes(1);

                return;
            }

            if (target.IsMonsterAggroDisabled())
            {
                return;
            }

            if (!monsterEntity.CanSeeInvisible && target.IsInvisible() && !isByAttacking)
            {
                return;
            }

            monsterEntity.Waypoints = null;
            monsterEntity.AttentionTime = time + TimeSpan.FromMinutes(1);
            monsterEntity.NextTick -= TimeSpan.FromMilliseconds(800);
            monsterEntity.GoToBossPosition = null;

            switch (target)
            {
                case IMateEntity:
                case IPlayerEntity:
                    AggroLogic(monsterEntity, target, isByAttacking, time);
                    break;
                default:
                    if (!monsterEntity.Targets.Contains(target))
                    {
                        monsterEntity.Targets.Add(target);
                    }

                    if (!monsterEntity.TargetsByVisualTypeAndId.Contains((target.Type, target.Id)))
                    {
                        monsterEntity.TargetsByVisualTypeAndId.Add((target.Type, target.Id));
                    }

                    TryApproachToClosestTarget(monsterEntity);

                    if (!isByAttacking)
                    {
                        return;
                    }

                    if (monsterEntity.Damagers.Contains(target))
                    {
                        return;
                    }

                    monsterEntity.Damagers.Add(target);
                    break;
            }
        }

        public void ForgetAll(IMonsterEntity monsterEntity, in DateTime time, bool clearDamagers = true)
        {
            if (monsterEntity.Target != null)
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
            }

            monsterEntity.LastSkill = DateTime.MinValue;

            if (monsterEntity.IsTotem)
            {
                monsterEntity.BroadcastEffectGround(0, monsterEntity.PositionX, monsterEntity.PositionY, true);
            }

            if (monsterEntity.GroupAttack != (int)GroupAttackType.None)
            {
                ForgetAllDamagersFromGroupAttackers(monsterEntity);
            }

            monsterEntity.PlayersDamage.Clear();

            if (clearDamagers)
            {
                monsterEntity.Damagers.Clear();
            }

            monsterEntity.Targets.Clear();
            monsterEntity.TargetsByVisualTypeAndId.Clear();
        }

        public IMonsterEntity GetMonsterById(long id) => _monstersById.GetOrDefault(id);
        public IMonsterEntity GetMonsterByUniqueId(Guid id) => _monstersByUniqueId.GetOrDefault(id);
        public List<IMonsterEntity> GetMonstersAlive() => _monsters.FindAll(x => x != null && x.IsAlive() && x.IsStillAlive && !x.IsBoss);

        public List<IMonsterEntity> GetRemainingTotems()
        {
            var totems = GetMonstersAlive()
                .Where(x => x.IsTotem)
                .ToList();
            return totems;
        }

        public IReadOnlyList<IMonsterEntity> GetAliveMonsters()
        {
            _lock.EnterReadLock();
            try
            {
                return _monsters.FindAll(x => x != null && x.IsAlive() && x.IsStillAlive);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IMonsterEntity> GetDeadMonsters()
        {
            _lock.EnterReadLock();
            try
            {
                return _monsters.FindAll(x => !x.IsAlive() || !x.IsStillAlive);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IMonsterEntity> GetAliveMonsters(Func<IMonsterEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _monsters.FindAll(entity => entity is { IsStillAlive: true } && entity.IsAlive() && (predicate == null || predicate(entity)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IMonsterEntity> GetAliveMonstersInRange(Position pos, short distance)
        {
            return GetAliveMonsters(s => pos.IsInAoeZone(s.Position, (byte)(distance + s.CellSize)));
        }

        public IReadOnlyList<IMonsterEntity> GetClosestMonstersInRange(Position pos, short distance)
        {
            _lock.EnterReadLock();
            try
            {
                List<IMonsterEntity> toReturn = _monsters.FindAll(s => s != null && s.IsAlive() && s.IsStillAlive && pos.IsInAoeZone(s.Position, distance));
                toReturn.Sort((prev, next) => prev.Position.GetDistance(pos) - next.Position.GetDistance(pos));

                return toReturn;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void AddMonster(IMonsterEntity entity)
        {
            _toAddMonsters.Enqueue(entity);
        }

        private void RemoveTotems(IMapInstance mapInstance, IMonsterEntity totem, DateTime date)
        {
            ForgetAll(totem, date);
            totem.BroadcastDie();
            mapInstance.RemoveMonster(totem);
        }

        public void RemoveMonster(IMonsterEntity entity)
        {
            _toRemoveMonsters.Enqueue(entity);
        }

        private bool ProcessLaurenaTick(DateTime date, IMonsterEntity monsterEntity, bool isTickRefresh)
        {
            if (monsterEntity.MonsterVNum == (short)MonsterVnum.LAURENA)
            {
                var laurenaCaligors = monsterEntity.MapInstance.GetAliveMonsters()
                    .Where(s => s.MonsterVNum == (short)MonsterVnum.LAURENA_CALIGOR)
                    .ToList();

                if (laurenaCaligors.Count == 0)
                {
                    return true;
                }

                return false;
            }

            return true;
        }


        private void Update(in DateTime date, IMonsterEntity monsterEntity, bool isTickRefresh)
        {
            if (!monsterEntity.IsStillAlive)
            {
                ProcessRespawnLogic(monsterEntity, date);
                return;
            }

            if (monsterEntity.SpawnDate.AddMilliseconds(500) > date)
            {
                if (monsterEntity.SummonerType is not VisualType.Player)
                {
                    return;
                }
            }

            if (!monsterEntity.IsAlive())
            {
                return;
            }

            bool isLaurenaGodmode = ProcessLaurenaTick(date, monsterEntity, isTickRefresh);

            if (monsterEntity.MonsterVNum == (int)MonsterVnum.DEMON_GOD_BELIAL)
            {
                if ((date - _lastCheckTime).TotalMilliseconds < 100)
                {
                    return;
                }

                _lastCheckTime = date;
                List<IMonsterEntity> totems = monsterEntity.MapInstance.GetRemainingTotems();
                bool allTotemsRemoved = true;
                var playersInAnyWhiteTotemZone = new HashSet<IPlayerEntity>();
                bool anyWhiteTotemRemoved = false;

                foreach (IMonsterEntity totem in totems)
                {
                    if (!_totemSpawnTime.TryGetValue(totem, out DateTime value))
                    {
                        value = date;
                        _totemSpawnTime[totem] = value;
                    }

                    bool isBlackTotem = totem.MonsterVNum == (int)MonsterVnum.BELIAL_DARK_TOTEM;
                    TimeSpan totemDuration = date - value;

                    if (!isBlackTotem)
                    {
                        IReadOnlyList<IPlayerEntity> playersInZone = monsterEntity.MapInstance.GetAliveCharactersInRange(totem.Position, 5);
                        playersInAnyWhiteTotemZone.UnionWith(playersInZone);
                    }

                    switch (isBlackTotem)
                    {
                        case true when totemDuration.TotalMilliseconds >= BLACK_TOTEM_DURATION:
                        case false when totemDuration.TotalMilliseconds >= WHITE_TOTEM_DURATION:
                        {
                            RemoveTotems(monsterEntity.MapInstance, totem, date);
                            _totemSpawnTime.Remove(totem);
                            if (!isBlackTotem)
                            {
                                anyWhiteTotemRemoved = true;
                            }

                            break;
                        }
                        case false:
                            allTotemsRemoved = false;
                            break;
                    }

                    if (isBlackTotem)
                    {
                        IReadOnlyList<IPlayerEntity> playersInZone = monsterEntity.MapInstance.GetAliveCharactersInRange(totem.Position, 5);
                        foreach (IPlayerEntity player in playersInZone)
                        {
                            if (_playersInBlackTotemZone.Contains(player))
                            {
                                continue;
                            }

                            Buff buff = _buff.CreateBuff((int)BuffVnums.SUDDEN_TWIST, monsterEntity);
                            if (player.HasBuff(BuffVnums.SUDDEN_TWIST))
                            {
                                Task.Delay(200);
                            }

                            player.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                            _playersInBlackTotemZone.Add(player);
                        }

                        var playersToRemove = _playersInBlackTotemZone.Where(p => !playersInZone.Contains(p)).ToList();
                        foreach (IPlayerEntity player in playersToRemove)
                        {
                            _playersInBlackTotemZone.Remove(player);
                        }
                    }
                }

                if (anyWhiteTotemRemoved)
                {
                    IReadOnlyList<IPlayerEntity> allPlayers = monsterEntity.MapInstance.GetAliveCharacters();
                    IEnumerable<IPlayerEntity> playersOutsideAllWhiteTotemZones = allPlayers.Except(playersInAnyWhiteTotemZone);

                    foreach (IPlayerEntity player in playersOutsideAllWhiteTotemZones)
                    {
                        if (_playersAlreadyDamaged.Contains(player))
                        {
                            continue;
                        }

                        double damageAmount = player.MaxHp * 0.8;
                        int damage = (int)damageAmount;
                        if (player.Hp - damage <= 0)
                        {
                            player.BroadcastDamage(damage);
                            player.Hp = 1;
                        }
                        else
                        {
                            player.BroadcastDamage(damage);
                            player.Hp -= damage;
                        }

                        monsterEntity.BroadcastCleanSuPacket(player, damage);
                        player.Session.RefreshStat();
                        _playersAlreadyDamaged.Add(player);
                    }
                }

                if (allTotemsRemoved)
                {
                    _playersAlreadyDamaged.Clear();
                }
            }

            ShowEffect(monsterEntity, date);
            _bCardTickSystem.ProcessUpdate(monsterEntity, date);
            ProcessRecurrentLifeDecrease(monsterEntity, date);
            ProcessManaRegeneration(monsterEntity, date);
            TryRemoveTargets(monsterEntity, date);

            if (_mapInstance.AIDisabled)
            {
                return;
            }

            if (monsterEntity.IsCastingSkill)
            {
                IBattleEntity entity = monsterEntity.MapInstance.GetBattleEntity(monsterEntity.LastAttackedEntity.Item1, monsterEntity.LastAttackedEntity.Item2);
                if (entity == null)
                {
                    monsterEntity.CancelCastingSkill();
                }

                return;
            }

            if (monsterEntity.IsMonsterAggroDisabled())
            {
                return;
            }

            if (monsterEntity.NextTick > date)
            {
                return;
            }

            TryApproachToClosestTarget(monsterEntity);
            RefreshTarget(monsterEntity, date);
            TryRunAway(monsterEntity, date);
            TryTalk(monsterEntity);

            if (monsterEntity.IsApproachingTarget)
            {
                monsterEntity.IsApproachingTarget = false;
                ApproachTarget(monsterEntity, date);
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(1000);
                return;
            }

            if (monsterEntity.FindNewPositionAroundTarget)
            {
                monsterEntity.FindNewPositionAroundTarget = false;
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);

                if (monsterEntity.Target == null)
                {
                    return;
                }

                short randomX = (short)(monsterEntity.Target.Position.X + _randomGenerator.RandomNumber(-1, 2));
                short randomY = (short)(monsterEntity.Target.Position.Y + _randomGenerator.RandomNumber(-1, 2));

                if (randomX == monsterEntity.Position.X && randomY == monsterEntity.Position.Y)
                {
                    return;
                }

                if (randomX == monsterEntity.Target.Position.X && randomY == monsterEntity.Target.PositionY)
                {
                    monsterEntity.FindNewPositionAroundTarget = true;
                    return;
                }

                if (!MovementPreChecks(monsterEntity))
                {
                    return;
                }

                if (monsterEntity.MapInstance.IsBlockedZone(randomX, randomY))
                {
                    return;
                }

                if (monsterEntity.IsMonsterAggroDisabled(randomX, randomY))
                {
                    return;
                }

                ProcessMovement(monsterEntity, randomX, randomY);
                return;
            }

            if (monsterEntity.Target == null ||
                (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.RaidInstance && (monsterEntity.GoToBossPosition != null || monsterEntity.Waypoints != null)))
            {
                int random = _randomGenerator.RandomNumber();
                bool move = random <= 60;

                if (move || monsterEntity.ReturningToFirstPosition || monsterEntity.GoToBossPosition != null || monsterEntity.Waypoints != null)
                {
                    WalkAround(monsterEntity, date);
                }

                monsterEntity.NextTick = (isTickRefresh ? date + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000)) : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(1000);
                return;
            }

            TryFight(date, monsterEntity, isTickRefresh, isLaurenaGodmode);
            CheckQuestMonster(monsterEntity, date);
            CheckForModeRange(monsterEntity);
        }

        private void TryTalk(IMonsterEntity monsterEntity)
        {
            if (!_monsterTalkingConfig.HasPossibleMessages(monsterEntity.MonsterVNum))
            {
                return;
            }

            if (_randomGenerator.RandomNumber() > 5)
            {
                return;
            }

            IReadOnlyList<string> messages = _monsterTalkingConfig.PossibleMessage(monsterEntity.MonsterVNum);
            if (messages == null)
            {
                return;
            }

            if (messages.Count < 1)
            {
                return;
            }

            string message = messages[_randomGenerator.RandomNumber(messages.Count)];
            monsterEntity.MapInstance.Broadcast(x => monsterEntity.GenerateSayPacket(x.GetLanguage(message), ChatMessageColorType.PlayerSay),
                new RangeBroadcast(monsterEntity.PositionX, monsterEntity.PositionY, 30));
        }

        private void TryRunAway(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (!monsterEntity.IsRunningAway)
            {
                return;
            }

            ApproachTarget(monsterEntity, date);
            monsterEntity.NextTick += TimeSpan.FromMilliseconds(1000);
        }

        private void TryRemoveTargets(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (monsterEntity.Targets.Count == 0 && monsterEntity.TargetsByVisualTypeAndId.Count == 0)
            {
                return;
            }

            if (monsterEntity.LastTargetsRefresh.AddSeconds(5) > date)
            {
                return;
            }

            monsterEntity.LastTargetsRefresh = date;
            HashSet<IBattleEntity> toRemove = new();
            HashSet<(VisualType, long)> toRemoveByTypeAndId = new();

            foreach ((VisualType visualType, long id) in monsterEntity.TargetsByVisualTypeAndId)
            {
                IBattleEntity entityOnMap = monsterEntity.MapInstance.GetBattleEntity(visualType, id);
                if (entityOnMap != null && entityOnMap.IsAlive())
                {
                    continue;
                }

                toRemoveByTypeAndId.Add((visualType, id));
            }

            foreach (IBattleEntity target in monsterEntity.Targets)
            {
                if (target == null)
                {
                    continue;
                }

                IBattleEntity entityOnMap = monsterEntity.MapInstance.GetBattleEntity(target.Type, target.Id);
                if (entityOnMap != null && entityOnMap.IsAlive())
                {
                    continue;
                }

                toRemove.Add(target);
            }

            foreach ((VisualType, long) entity in toRemoveByTypeAndId)
            {
                monsterEntity.TargetsByVisualTypeAndId.Remove((entity.Item1, entity.Item2));
            }

            foreach (IBattleEntity entity in toRemove)
            {
                RemoveTarget(monsterEntity, entity);
            }
        }

        private void ProcessManaRegeneration(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (!monsterEntity.CanRegenMp)
            {
                return;
            }

            if (monsterEntity.Mp == monsterEntity.MaxMp)
            {
                return;
            }

            if (monsterEntity.LastMpRegen.AddSeconds(3) > date)
            {
                return;
            }

            monsterEntity.LastMpRegen = date;
            int toAdd = (int)(monsterEntity.MaxMp * 0.01);
            monsterEntity.Mp = monsterEntity.Mp + toAdd > monsterEntity.MaxMp ? monsterEntity.MaxMp : monsterEntity.Mp + toAdd;
        }

        private void TryApproachToClosestTarget(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.TargetsByVisualTypeAndId.Count == 0)
            {
                return;
            }

            IBattleEntity newEntity = monsterEntity.MapInstance
                .GetBattleEntities(x => monsterEntity.CanHit(x) && monsterEntity.IsEnemyWith(x) && monsterEntity.TargetsByVisualTypeAndId.Contains((x.Type, x.Id)))
                .OrderBy(x => monsterEntity.Position.GetDistance(x.Position)).FirstOrDefault();

            if (newEntity == null)
            {
                return;
            }

            monsterEntity.Target = newEntity;
        }

        private void CheckQuestMonster(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (!monsterEntity.IsMonsterSpawningMonstersForQuest())
            {
                return;
            }

            if (date <= monsterEntity.NextAttackReady)
            {
                return;
            }

            ForgetAll(monsterEntity, date);
        }

        private void TryActivateMode(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.EventGameInstance)
            {
                return;
            }

            if (monsterEntity.ModeIsActive)
            {
                return;
            }

            if (monsterEntity.ModeLimiterType == 1)
            {
                if (monsterEntity.Position.GetDistance(monsterEntity.Target.Position) > monsterEntity.ModeRangeTreshold)
                {
                    return;
                }
            }

            if (monsterEntity.ModeIsHpTriggered && monsterEntity.GetHpPercentage() > monsterEntity.ModeHpTresholdOrItemVnum)
            {
                return;
            }

            ActivateMode(monsterEntity);
        }

        private void TryDeactivateMode(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.EventGameInstance)
            {
                return;
            }

            if (!monsterEntity.ModeIsActive)
            {
                return;
            }

            if (monsterEntity.ModeLimiterType == 2)
            {
                if (_totalMonstersDeaths - monsterEntity.ModeDeathsSinceRespawn < monsterEntity.ModeRangeTreshold)
                {
                    return;
                }
            }

            DeactivateMode(monsterEntity);
        }

        private void CheckForModeRange(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.EventGameInstance)
            {
                return;
            }

            if (!monsterEntity.HasMode)
            {
                return;
            }

            if (!monsterEntity.ModeIsHpTriggered)
            {
                return;
            }

            if (monsterEntity.ModeLimiterType != 1)
            {
                return;
            }

            if (monsterEntity.Target == null)
            {
                DeactivateMode(monsterEntity);
                return;
            }

            if (monsterEntity.Position.GetDistance(monsterEntity.Target.Position) <= monsterEntity.ModeRangeTreshold)
            {
                return;
            }

            DeactivateMode(monsterEntity);
        }

        private void AddModeBCards(IMonsterEntity monsterEntity)
        {
            IReadOnlyList<BCardDTO> modeBCards = monsterEntity.ModeBCards;

            var attackBCards = new List<BCardDTO>();
            var defenseBCards = new List<BCardDTO>();

            foreach (BCardDTO bCard in modeBCards)
            {
                switch ((CastType)bCard.CastType)
                {
                    case CastType.SELF:
                        monsterEntity.BCardComponent.AddBCard(bCard);
                        _bcardHandlers.Execute(monsterEntity, monsterEntity, bCard);
                        break;
                    case CastType.ATTACK:
                        attackBCards.Add(bCard);
                        break;
                    case CastType.DEFENSE:
                        defenseBCards.Add(bCard);
                        break;
                }
            }

            monsterEntity.BCardComponent.AddTriggerBCards(BCardTriggerType.ATTACK, attackBCards);
            monsterEntity.BCardComponent.AddTriggerBCards(BCardTriggerType.DEFENSE, defenseBCards);
            monsterEntity.RefreshStats();
        }

        private void RemoveModeBCards(IMonsterEntity monsterEntity)
        {
            IReadOnlyList<BCardDTO> modeBCards = monsterEntity.ModeBCards;

            foreach (BCardDTO bCard in modeBCards.Where(x => (CastType)x.CastType == CastType.SELF))
            {
                monsterEntity.BCardComponent.RemoveBCard(bCard);
            }

            monsterEntity.BCardComponent.RemoveAllTriggerBCards(BCardTriggerType.ATTACK);
            monsterEntity.BCardComponent.RemoveAllTriggerBCards(BCardTriggerType.DEFENSE);
            monsterEntity.RefreshStats();
        }


        private void ProcessRecurrentLifeDecrease(IMonsterEntity monsterEntity, DateTime date)
        {
            if (monsterEntity.LastSpecialHpDecrease.AddSeconds(1) > date)
            {
                return;
            }

            bool isSummonedByAnotherMonster = monsterEntity.SummonerId != 0 && monsterEntity.SummonerType == VisualType.Monster;

            monsterEntity.LastSpecialHpDecrease = date;
            if (monsterEntity.DisappearAfterSeconds || isSummonedByAnotherMonster)
            {
                int hpToDecrease = monsterEntity.MaxHp / (monsterEntity.MaxHp / 5);
                monsterEntity.Hp -= hpToDecrease;
                if (monsterEntity.Hp > 0)
                {
                    return;
                }

                _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monsterEntity));
                return;
            }

            if (!monsterEntity.DisappearAfterSecondsMana && !monsterEntity.IsSparringMonster)
            {
                return;
            }

            if (monsterEntity.DisappearAfterSecondsMana)
            {
                int toRemove = monsterEntity.MaxMp / (monsterEntity.MaxMp / 10);
                monsterEntity.Mp -= toRemove;
            }

            if (monsterEntity.IsSparringMonster)
            {
                monsterEntity.Mp -= 1;
            }

            if (monsterEntity.Mp > 0)
            {
                return;
            }

            _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monsterEntity));
        }

        private void RefreshTarget(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (monsterEntity.BCardComponent.HasBCard(BCardType.SpecialEffects, (byte)AdditionalTypes.SpecialEffects.ToNonPrefferedAttack))
            {
                return;
            }

            IBattleEntity target = monsterEntity.Target;
            // if someone attack monster
            if (target != null)
            {
                // if target is on diffrent map 
                // if target is dead
                if (target.MapInstance?.Id != monsterEntity.MapInstance?.Id)
                {
                    RemoveTarget(monsterEntity, target);
                    return;
                }

                if (!target.IsAlive())
                {
                    RemoveTarget(monsterEntity, target);
                    return;
                }
            }

            // if monster is agressive, find target || looking for whether or not his companions were attacked
            if (!monsterEntity.IsHostile && monsterEntity.GroupAttack == (int)GroupAttackType.None || target != null)
            {
                return;
            }

            FindTarget(monsterEntity, date);
        }

        private bool IsAggroLimitReached(IMonsterEntity monsterEntity, IBattleEntity entity, bool addValue = true, bool isByAttacking = false)
        {
            if (entity == null)
            {
                return false;
            }

            if (monsterEntity.IsMateTrainer)
            {
                return false;
            }

            if (monsterEntity.MapInstance.HasMapFlag(MapFlags.IS_SPECIAL_MAP))
            {
                entity.AggroedEntities.Add(monsterEntity.UniqueId);
                return false;
            }

            if (monsterEntity.RawHostility is (int)HostilityType.ATTACK_ANGELS_ONLY or (int)HostilityType.ATTACK_DEVILS_ONLY)
            {
                return false;
            }

            if (!monsterEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                return false;
            }

            if (monsterEntity.IsMonsterSpawningMonstersForQuest())
            {
                return false;
            }

            if (!monsterEntity.CanWalk)
            {
                return false;
            }

            if (entity.AggroedEntities.Count >= AGGRO_LIMIT_PER_ENTITY && !isByAttacking)
            {
                return true;
            }

            if (!addValue)
            {
                return false;
            }

            entity.AggroedEntities.Add(monsterEntity.UniqueId);
            return false;
        }

        private void AggroIsRemoved(IMonsterEntity monsterEntity, IBattleEntity target)
        {
            if (!monsterEntity.IsAlive())
            {
                foreach (IBattleEntity targetInTargets in monsterEntity.Targets)
                {
                    if (targetInTargets == null)
                    {
                        continue;
                    }

                    targetInTargets.AggroedEntities?.Remove(monsterEntity.UniqueId);
                    monsterEntity.TargetsByVisualTypeAndId.Remove((targetInTargets.Type, targetInTargets.Id));
                }

                return;
            }

            if (target == null)
            {
                return;
            }

            target.AggroedEntities?.Remove(monsterEntity.UniqueId);
            monsterEntity.TargetsByVisualTypeAndId.Remove((target.Type, target.Id));
        }

        private void FindTarget(IMonsterEntity monsterEntity, in DateTime time, bool ignoreHostile = false)
        {
            IBattleEntity target = null;

            if (monsterEntity.IsHostile || ignoreHostile)
            {
                target = HostileFinding(monsterEntity);
            }
            else if (monsterEntity.GroupAttack != (int)GroupAttackType.None)
            {
                target = DefensiveFinding(monsterEntity);
            }

            if (target == null)
            {
                return;
            }

            if (target.BCardComponent.HasBCard(BCardType.SpecialBehaviour, (byte)AdditionalTypes.SpecialBehaviour.IgnoreTarget))
            {
                FishingSpotDto spot = _fishManager.GetFishSpotByMapId(monsterEntity.MapInstance.MapId);
                if (spot != null)
                {
                    return;
                }
            }

            MonsterRefreshTarget(monsterEntity, target, time);
        }

        private IBattleEntity HostileFinding(IMonsterEntity monsterEntity)
        {
            IEnumerable<IBattleEntity> targets;
            short noticeRange = monsterEntity.NoticeRange;

            switch (monsterEntity.RawHostility)
            {
                case (int)HostilityType.ATTACK_MATES:
                    targets = monsterEntity.MapInstance.GetAliveMatesInRange(monsterEntity.Position, noticeRange);
                    break;
                case (int)HostilityType.ATTACK_IN_RANGE:
                    targets = monsterEntity.MapInstance.GetBattleEntitiesInRange(monsterEntity.Position, noticeRange);
                    break;
                case (int)HostilityType.ATTACK_ANGELS_ONLY:
                    targets = monsterEntity.MapInstance.GetNonMonsterBattleEntitiesInRange(monsterEntity.Position, noticeRange, x => x.Faction == FactionType.Angel);
                    break;
                case (int)HostilityType.ATTACK_DEVILS_ONLY:
                    targets = monsterEntity.MapInstance.GetNonMonsterBattleEntitiesInRange(monsterEntity.Position, noticeRange, x => x.Faction == FactionType.Demon);
                    break;
                case (int)HostilityType.ATTACK_NOT_WEARING_PHANTOM_AMULET:
                    targets = monsterEntity.MapInstance.GetCharactersInRange(monsterEntity.Position, noticeRange, x => !monsterEntity.FindPhantomAmulet(x));
                    break;
                case (int)HostilityType.NOT_HOSTILE:
                    targets = monsterEntity.MapInstance.GetBattleEntitiesInRange(monsterEntity.Position, noticeRange);
                    break;
                default:
                    if (monsterEntity.IsMonsterSpawningMonstersForQuest() == false)
                    {
                        throw new ArgumentOutOfRangeException("HostilityType",
                            $"The HostilityType: '{monsterEntity.RawHostility.ToString()}' is not being handled in the actual switch inside MonsterAISystem.FindTarget");
                    }

                    int questId = monsterEntity.RawHostility - 20000;
                    targets = monsterEntity.MapInstance.GetCharactersInRange(monsterEntity.Position, 1, s => s.HasQuestWithId(questId) && !s.IsQuestCompleted(s.GetQuestById(questId)));
                    break;
            }

            return BasicTargetChecks(monsterEntity, targets);
        }

        private IBattleEntity BasicTargetChecks(IMonsterEntity monsterEntity, IEnumerable<IBattleEntity> targets)
        {
            if (monsterEntity.SummonerId.HasValue && monsterEntity.SummonerType.HasValue)
            {
                IBattleEntity summoner = monsterEntity.MapInstance.GetBattleEntity(monsterEntity.SummonerType.Value, monsterEntity.SummonerId.Value);
                if (monsterEntity.SummonerType.Value == VisualType.Player && summoner != null)
                {
                    targets = monsterEntity.GetEnemiesInRange(monsterEntity, monsterEntity.NoticeRange);
                }
                else
                {
                    targets = targets.Where(monsterEntity.IsEnemyWith);
                }
            }
            else
            {
                targets = targets.Where(monsterEntity.IsEnemyWith);
            }

            if (monsterEntity.SummonerId == null)
            {
                targets = targets.Where(e =>
                {
                    if (monsterEntity.IsMonsterSpawningMonstersForQuest() && !e.IsPlayer())
                    {
                        return false;
                    }

                    if (monsterEntity.CanHit(e) && e.IsAlive() && !IsAggroLimitReached(monsterEntity, e, false) && !e.IsMonsterAggroDisabled())
                    {
                        return monsterEntity.CanSeeInvisible || !e.IsInvisible();
                    }

                    return false;
                });
            }

            return targets.OrderBy(monsterEntity.GetDistance).FirstOrDefault();
        }

        private IBattleEntity DefensiveFinding(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.Target != null)
            {
                return monsterEntity.Target;
            }

            int monsterBy = monsterEntity.GroupAttack == (int)GroupAttackType.ByRace ? (int)monsterEntity.MonsterRaceType : monsterEntity.IconId;

            if (!_attackerByRace.TryGetValue(monsterBy, out Dictionary<long, long> dictionary))
            {
                return default;
            }

            short noticeRange = monsterEntity.NoticeRange;
            IEnumerable<IPlayerEntity> targets = monsterEntity.MapInstance.GetCharactersInRange(monsterEntity.Position, noticeRange,
                e => dictionary.ContainsKey(e.Id) && dictionary.GetOrDefault(e.Id) > 0);

            return BasicTargetChecks(monsterEntity, targets);
        }

        private void AggroLogicGroupAttackType(IMonsterEntity monsterEntity, IBattleEntity entity, bool isByAttacking, in DateTime time)
        {
            if (IsAggroLimitReached(monsterEntity, entity, true, isByAttacking))
            {
                return;
            }

            AddAttackerToGroupAttackers(monsterEntity, entity);
            if (!isByAttacking)
            {
                monsterEntity.LastSkill = time;
            }

            AddPlayerOrMateTarget(monsterEntity, entity, isByAttacking);
            TryApproachToClosestTarget(monsterEntity);

            if (isByAttacking)
            {
                return;
            }

            if (entity is not IPlayerEntity playerEntity)
            {
                return;
            }

            playerEntity.Session?.SendEffectEntity(monsterEntity, EffectType.TargetedByOthers);
        }

        private bool AddPlayerOrMateTarget(IMonsterEntity monsterEntity, IBattleEntity entity, bool isByAttacking)
        {
            bool removeTickFromMonster = false;

            long? dollOwnerId = monsterEntity.SummonerId;

            switch (entity)
            {
                case IMateEntity mateEntity:
                    if (monsterEntity.IsMonsterSpawningMonstersForQuest())
                    {
                        return true;
                    }

                    IPlayerEntity owner = mateEntity.Owner;
                    if (!monsterEntity.IsMateTrainer && !monsterEntity.IsSparringMonster)
                    {
                        if (!monsterEntity.Targets.Contains(owner))
                        {
                            monsterEntity.Targets.Add(owner);
                            removeTickFromMonster = true;
                        }

                        if (!monsterEntity.TargetsByVisualTypeAndId.Contains((owner.Type, owner.Id)))
                        {
                            monsterEntity.TargetsByVisualTypeAndId.Add((owner.Type, owner.Id));
                            removeTickFromMonster = true;
                        }

                        if (isByAttacking && !monsterEntity.Damagers.Contains(owner))
                        {
                            monsterEntity.Damagers.Add(owner);
                            removeTickFromMonster = true;
                        }
                    }
                    else if (dollOwnerId.HasValue && dollOwnerId.Value != owner.Id)
                    {
                        return true;
                    }

                    if (!monsterEntity.Targets.Contains(mateEntity))
                    {
                        monsterEntity.Targets.Add(mateEntity);
                        removeTickFromMonster = true;
                    }

                    if (!monsterEntity.TargetsByVisualTypeAndId.Contains((mateEntity.Type, mateEntity.Id)))
                    {
                        monsterEntity.TargetsByVisualTypeAndId.Add((mateEntity.Type, mateEntity.Id));
                        removeTickFromMonster = true;
                    }

                    if (isByAttacking && !monsterEntity.Damagers.Contains(mateEntity))
                    {
                        monsterEntity.Damagers.Add(mateEntity);
                        removeTickFromMonster = true;
                    }

                    IMateEntity secondMate = owner.MateComponent.GetTeamMember(x => x.Type != mateEntity.Type);
                    if (secondMate == null)
                    {
                        break;
                    }

                    if (!monsterEntity.Targets.Contains(secondMate))
                    {
                        monsterEntity.Targets.Add(secondMate);
                        removeTickFromMonster = true;
                    }

                    if (!monsterEntity.TargetsByVisualTypeAndId.Contains((secondMate.Type, secondMate.Id)))
                    {
                        monsterEntity.TargetsByVisualTypeAndId.Add((secondMate.Type, secondMate.Id));
                        removeTickFromMonster = true;
                    }

                    if (isByAttacking && !monsterEntity.Damagers.Contains(secondMate))
                    {
                        monsterEntity.Damagers.Add(secondMate);
                        removeTickFromMonster = true;
                    }

                    break;
                case IPlayerEntity playerEntity:
                    if (!monsterEntity.IsMateTrainer && !monsterEntity.IsSparringMonster)
                    {
                        if (!monsterEntity.Targets.Contains(playerEntity))
                        {
                            monsterEntity.Targets.Add(playerEntity);
                            removeTickFromMonster = true;
                        }

                        if (!monsterEntity.TargetsByVisualTypeAndId.Contains((playerEntity.Type, playerEntity.Id)))
                        {
                            monsterEntity.TargetsByVisualTypeAndId.Add((playerEntity.Type, playerEntity.Id));
                            removeTickFromMonster = true;
                        }
                    }
                    else if (dollOwnerId.HasValue && dollOwnerId.Value != playerEntity.Id)
                    {
                        return true;
                    }

                    if (monsterEntity.IsMonsterSpawningMonstersForQuest())
                    {
                        return true;
                    }

                    if (isByAttacking && !monsterEntity.Damagers.Contains(playerEntity))
                    {
                        monsterEntity.Damagers.Add(playerEntity);
                        removeTickFromMonster = true;
                    }

                    foreach (IMateEntity mate in playerEntity.MateComponent.TeamMembers())
                    {
                        if (!monsterEntity.Targets.Contains(mate))
                        {
                            monsterEntity.Targets.Add(mate);
                            removeTickFromMonster = true;
                        }

                        if (!monsterEntity.TargetsByVisualTypeAndId.Contains((mate.Type, mate.Id)))
                        {
                            monsterEntity.TargetsByVisualTypeAndId.Add((mate.Type, mate.Id));
                            removeTickFromMonster = true;
                        }

                        if (!isByAttacking)
                        {
                            continue;
                        }

                        if (monsterEntity.Damagers.Contains(mate))
                        {
                            continue;
                        }

                        monsterEntity.Damagers.Add(mate);
                        removeTickFromMonster = true;
                    }

                    break;
            }

            return removeTickFromMonster;
        }

        private void AddAttackerToGroupAttackers(IMonsterEntity monsterEntity, IBattleEntity target)
        {
            IBattleEntity entity = target switch
            {
                IMateEntity mate => mate.Owner,
                _ => target
            };

            int monsterBy = monsterEntity.GroupAttack == (int)GroupAttackType.ByRace ? (int)monsterEntity.MonsterRaceType : monsterEntity.IconId;
            if (_attackerByRace.TryGetValue(monsterBy, out Dictionary<long, long> dictionary))
            {
                if (!dictionary.TryGetValue(entity.Id, out long monsterRaceAggro))
                {
                    dictionary.Add(entity.Id, 1);
                    return;
                }

                if (monsterRaceAggro >= AGGRO_LIMIT_PER_ENTITY && !entity.MapInstance.HasMapFlag(MapFlags.IS_SPECIAL_MAP))
                {
                    return;
                }

                if (monsterRaceAggro >= AGGRO_LIMIT_PER_ENTITY)
                {
                    return;
                }

                monsterRaceAggro++;
                dictionary[entity.Id] = monsterRaceAggro;
                return;
            }

            _attackerByRace.Add(monsterBy, new Dictionary<long, long>
            {
                { entity.Id, 1 }
            });
        }

        private void RemoveTargetFromGroupAttackers(IMonsterEntity monsterEntity, IBattleEntity target)
        {
            if (target?.Type != VisualType.Player)
            {
                return;
            }

            int monsterBy = monsterEntity.GroupAttack == (int)GroupAttackType.ByRace ? (int)monsterEntity.MonsterRaceType : monsterEntity.IconId;
            if (!_attackerByRace.TryGetValue(monsterBy, out Dictionary<long, long> dictionary))
            {
                return;
            }

            if (!dictionary.TryGetValue(target.Id, out long amount))
            {
                return;
            }

            amount -= 1;
            dictionary[target.Id] = amount;

            bool removeFromDic = amount <= 0;

            if (!removeFromDic)
            {
                return;
            }

            _attackerByRace[monsterBy].Remove(target.Id);
        }

        private void ForgetAllDamagersFromGroupAttackers(IMonsterEntity monsterEntity)
        {
            int monsterBy = monsterEntity.GroupAttack == (int)GroupAttackType.ByRace ? (int)monsterEntity.MonsterRaceType : monsterEntity.IconId;
            if (!_attackerByRace.TryGetValue(monsterBy, out Dictionary<long, long> dictionary))
            {
                return;
            }

            foreach (IBattleEntity target in monsterEntity.Targets)
            {
                if (!dictionary.TryGetValue(target.Id, out long amount))
                {
                    continue;
                }

                amount -= 1;
                dictionary[target.Id] = amount;

                bool removeFromDic = amount <= 0;

                if (!removeFromDic)
                {
                    continue;
                }

                _attackerByRace[monsterBy].Remove(target.Id);
            }
        }

        private void AggroLogic(IMonsterEntity monsterEntity, IBattleEntity target, bool isByAttacking, in DateTime time)
        {
            if (monsterEntity.GroupAttack != (int)GroupAttackType.None)
            {
                AggroLogicGroupAttackType(monsterEntity, target, isByAttacking, time);
                return;
            }

            if (IsAggroLimitReached(monsterEntity, target, true, isByAttacking))
            {
                return;
            }

            if (!isByAttacking)
            {
                monsterEntity.LastSkill = time;
            }

            AddPlayerOrMateTarget(monsterEntity, target, isByAttacking);
            TryApproachToClosestTarget(monsterEntity);

            if (monsterEntity.MonsterVNum == (short)MonsterVnum.ONYX_MONSTER)
            {
                return;
            }

            switch (monsterEntity.MonsterRaceType)
            {
                case MonsterRaceType.Other:
                case MonsterRaceType.Fixed:
                    return;
            }

            if (!target.IsPlayer())
            {
                return;
            }

            if (monsterEntity.ReturningToFirstPosition)
            {
                return;
            }

            if (isByAttacking)
            {
                return;
            }

            if (monsterEntity.ShouldFindNewTarget)
            {
                return;
            }

            ((IPlayerEntity)target).Session.SendEffectEntity(monsterEntity, EffectType.Targeted);
        }

        private void TryFight(in DateTime date, IMonsterEntity monsterEntity, bool isTickRefresh, bool laurenaFight)
        {
            if (monsterEntity == null)
            {
                return;
            }

            if (monsterEntity.MonsterVNum == (short)MonsterVnum.ONYX_MONSTER)
            {
                return;
            }

            if (!monsterEntity.IsAlive())
            {
                return;
            }

            if (!monsterEntity.IsStillAlive)
            {
                return;
            }

            if (monsterEntity.Target == null)
            {
                return;
            }

            if (monsterEntity.IsRunningAway)
            {
                return;
            }

            if (!monsterEntity.Target.IsAlive())
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            if (!monsterEntity.CanSeeInvisible)
            {
                // if target is player and he become invisible
                if (monsterEntity.Target.IsInvisible())
                {
                    RemoveTarget(monsterEntity, monsterEntity.Target, true);
                    monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                    return;
                }
            }

            if (!monsterEntity.CanHit(monsterEntity.Target))
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            if (monsterEntity.MapInstance.IsPvp)
            {
                if (!monsterEntity.Target.IsInPvpZone())
                {
                    RemoveTarget(monsterEntity, monsterEntity.Target);
                    monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                    return;
                }
            }

            if (monsterEntity.Target is IMonsterEntity { IsJumping: true })
            {
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            bool isModeActive = monsterEntity.ModeIsActive;

            if (monsterEntity.ModeLimiterType == 2)
            {
                TryDeactivateMode(monsterEntity);
            }

            IReadOnlyList<INpcMonsterSkill> monsterSkills = monsterEntity.NotBasicSkills;

            IBattleEntitySkill dashSkill = null;
            if (monsterEntity.HasDash)
            {
                dashSkill = monsterEntity.DashSkill;
                monsterSkills = monsterEntity.SkillsWithoutDashSkill;
            }

            int count = monsterSkills.Count;
            IBattleEntitySkill getRandomSkill = count != 0 ? monsterSkills[_randomGenerator.RandomNumber(0, count)] : null;

            bool randomSkill = _randomGenerator.RandomNumber() <= getRandomSkill?.Rate;

            // Find basic skill that replace ZSKILL
            IBattleEntitySkill replacedBasic = monsterEntity.ReplacedBasicSkill;
            IBattleEntitySkill skillToUse = randomSkill ? getRandomSkill : replacedBasic;

            if (!laurenaFight && skillToUse?.Skill?.Id == (short)SkillsVnums.LAURENA_BASIC_ATTACK)
            {
                return;
            }

            // Special interactions
            if (skillToUse?.Skill?.Id is (short)SkillsVnums.DRAGON_STAGGER && !monsterEntity.HasSpawnRedCircle)
            {
                return;
            }

            bool forceUseSkill = monsterEntity.ForceUseSkill;
            if (monsterEntity.SkillToUse is not null)
            {
                skillToUse = monsterSkills.FirstOrDefault(x => x?.Skill.Id == monsterEntity.SkillToUse.Value);
                if (skillToUse == null && monsterEntity.DashSkill != null && monsterEntity.DashSkill.Skill.Id == monsterEntity.SkillToUse)
                {
                    skillToUse = dashSkill;
                }
            }

            SkillInfo skillInfo = skillToUse?.Skill.GetInfo(battleEntity: monsterEntity) ?? monsterEntity.BasicSkill;

            bool randomSkillCantBeUsed = skillToUse != null && !monsterEntity.SkillCanBeUsed(skillToUse, date) && skillInfo.Vnum != 0 && skillToUse != replacedBasic;

            if (randomSkillCantBeUsed && !forceUseSkill)
            {
                skillToUse = replacedBasic;
                skillInfo = skillToUse?.Skill.GetInfo(battleEntity: monsterEntity) ?? monsterEntity.BasicSkill;
            }

            int effectiveRange = skillInfo.Range == 0 || skillInfo.TargetType == TargetType.Self ? skillInfo.AoERange : skillInfo.Range;
            effectiveRange = skillInfo.TargetType == TargetType.Self &&
                (skillInfo.HitType == TargetHitType.TargetOnly || (skillInfo.HitType == TargetHitType.EnemiesInAffectedAoE && skillInfo.Range == 0))
                    ? byte.MaxValue
                    : effectiveRange;

            bool isInRange = monsterEntity.IsInRange(monsterEntity.Target.PositionX, monsterEntity.Target.PositionY, (byte)effectiveRange);

            if (monsterEntity.MapInstance.GetRemainingTotems().Count > 0 && monsterEntity.IsBoss
                && monsterEntity.MonsterVNum == (short)MonsterVnum.DEMON_GOD_BELIAL
                && skillInfo.Vnum is (short)SkillsVnums.BELIAL_DARK_TOTEM or (short)SkillsVnums.BELIAL_LIGHT_TOTEM)
            {
                return;
            }

            if (monsterEntity.MapInstance.GetMonstersAlive().Count > 0 && monsterEntity.IsBoss && monsterEntity.MonsterVNum == (short)MonsterVnum.TWISTED_BEAST_KING_CARNO)
            {
                skillInfo = monsterEntity.Skills.FirstOrDefault(x => x.Skill.Id == (short)SkillsVnums.CARNO_JUMP).Skill.GetInfo();
                forceUseSkill = true;
            }
            else
            {
                if (monsterEntity.SkillToUse is not null)
                {
                    skillToUse = monsterSkills.FirstOrDefault(x => monsterEntity.SkillToUse != null && x?.Skill.Id == monsterEntity.SkillToUse.Value);
                    if (skillToUse == null && monsterEntity.DashSkill != null && monsterEntity.DashSkill.Skill.Id == monsterEntity.SkillToUse)
                    {
                        skillToUse = dashSkill;
                    }
                }

                skillInfo = skillToUse?.Skill.GetInfo(battleEntity: monsterEntity) ?? monsterEntity.BasicSkill;
            }

            bool shouldWalk = skillInfo.TargetAffectedEntities !=
                TargetAffectedEntities.BuffForAllies; //skillInfo.TargetType == TargetType.Target && skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies ||
            //skillInfo.TargetType == TargetType.Self && skillInfo.HitType is TargetHitType.AlliesInAffectedAoE or TargetHitType.EnemiesInAffectedAoE;

            if (!isInRange && shouldWalk && !forceUseSkill && !monsterEntity.IgnoreSkillRange)
            {
                if (monsterEntity.HasDash && dashSkill != null)
                {
                    skillToUse = dashSkill;
                    skillInfo = dashSkill.Skill.GetInfo();

                    effectiveRange = skillInfo.Range == 0 || skillInfo.TargetType == TargetType.Self ? skillInfo.AoERange : skillInfo.Range;
                    if (!monsterEntity.SkillCanBeUsed(skillToUse, date) || !monsterEntity.CanPerformAttack()
                        || !monsterEntity.IsInRange(monsterEntity.Target.PositionX, monsterEntity.Target.PositionY, (byte)effectiveRange)
                        && skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies)
                    {
                        monsterEntity.IsApproachingTarget = true;
                        monsterEntity.NextTick = date;
                        return;
                    }

                    if (_randomGenerator.RandomNumber() > skillToUse.Rate)
                    {
                        monsterEntity.IsApproachingTarget = true;
                        monsterEntity.NextTick = date;
                        return;
                    }
                }
                else
                {
                    monsterEntity.IsApproachingTarget = true;
                    monsterEntity.NextTick = date;
                    return;
                }
            }

            monsterEntity.LastSkill = date;
            monsterEntity.AttentionTime = date + TimeSpan.FromSeconds(15);

            if (monsterEntity.NextAttackReady > date)
            {
                return;
            }

            if (getRandomSkill != null && getRandomSkill != monsterEntity.ReplacedBasicSkill && !randomSkill && monsterEntity.SkillCanBeUsed(getRandomSkill, date))
            {
                monsterEntity.SetSkillCooldown(getRandomSkill.Skill.GetInfo());
            }

            int random = _randomGenerator.RandomNumber();
            if ((monsterEntity.BasicHitChance == 0 || random >= monsterEntity.BasicHitChance * 20) && !forceUseSkill)
            {
                if (skillToUse is null or INpcMonsterSkill { IsIgnoringHitChance: false })
                {
                    monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                    return;
                }
            }

            // Invisible monsters who spawns monsters for quest
            if (monsterEntity.IsMonsterSpawningMonstersForQuest())
            {
                // check, if in notice range of monster there is a monster, who has been already spawned
                if (GetAliveMonstersInRange(monsterEntity.Position, monsterEntity.NoticeRange).Any(x => x.MonsterVNum == monsterEntity.SpawnMobOrColor))
                {
                    return;
                }

                int questVnum = monsterEntity.RawHostility - 20000;
                if (monsterEntity.Target is IPlayerEntity character)
                {
                    if (!character.HasQuestWithId(questVnum))
                    {
                        ForgetAll(monsterEntity, date);
                        return;
                    }
                }
                else
                {
                    string targetString = monsterEntity.Target == null ? "target is null" : $"{monsterEntity.Target.Type}:{monsterEntity.Target.Id}:{monsterEntity.Target.IsMate().ToString()}";
                    Log.Error($"[MONSTER_QUEST_SYSTEM][TryFight] Target does not have quest! {targetString}",
                        new MonsterQuestSystemException(
                            $"[MONSTER_QUEST_SYSTEM][TryFight] Target was not a player: {_mapInstance.MapId.ToString()}, {monsterEntity.MonsterVNum}, {monsterEntity.PositionX}, {monsterEntity.PositionY}"));
                    ForgetAll(monsterEntity, date);
                    return;
                }
            }

            IMonsterEntity monster = GetAliveMonstersInRange(monsterEntity.Position, 0).FirstOrDefault();
            if (monster != null && monsterEntity != monster && _randomGenerator.RandomNumber() <= 60 && MovementPreChecks(monsterEntity) && !forceUseSkill)
            {
                ProcessMovement(monsterEntity, monsterEntity.Target.Position.X, monsterEntity.Target.Position.Y);
                monsterEntity.NextTick += TimeSpan.FromMilliseconds(1000);
                return;
            }

            IBattleEntity skillTarget = monsterEntity.Target;

            (int firstData, int secondData) cooldownToIncrease = (0, 0);
            (int firstData, int secondData) cooldownToDecrease = (0, 0);

            if (monsterEntity.BCards.Count > 1)
            {
                cooldownToIncrease = monsterEntity.BCardComponent.GetAllBCardsInformation(BCardType.Mode, (byte)AdditionalTypes.Mode.AttackTimeIncreased, monsterEntity.Level);
                cooldownToDecrease = monsterEntity.BCardComponent.GetAllBCardsInformation(BCardType.Mode, (byte)AdditionalTypes.Mode.AttackTimeDecreased, monsterEntity.Level);
            }

            int basicCooldown = monsterEntity.BasicCooldown;

            if (cooldownToIncrease.firstData != 0)
            {
                basicCooldown += cooldownToIncrease.firstData;
            }

            if (cooldownToDecrease.firstData != 0)
            {
                basicCooldown -= cooldownToDecrease.firstData;
            }

            int tickToAdd = (2 + monsterEntity.BasicCastTime + 2 * basicCooldown) * 100;
            tickToAdd = tickToAdd < 800 ? 800 : tickToAdd;
            monsterEntity.NextAttackReady = date + TimeSpan.FromMilliseconds(tickToAdd);

            if (replacedBasic != null && skillInfo.Vnum == replacedBasic.Skill.Id && !monsterEntity.SkillCanBeUsed(replacedBasic, date) && !forceUseSkill)
            {
                return;
            }

            if (!monsterEntity.CanPerformAttack() && !forceUseSkill)
            {
                return;
            }

            if (!monsterEntity.Target.IsAlive())
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            bool check = CheckTargetHeroLevel(monsterEntity, date, isTickRefresh);
            if (check)
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
                monsterEntity.NextTick = (isTickRefresh ? date : monsterEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                IBattleEntity toFocus = monsterEntity.GetEnemiesInRange(monsterEntity, 5).FirstOrDefault(s =>
                {
                    if (s is not IMateEntity mateEntity)
                    {
                        return false;
                    }

                    return !CheckTargetHeroLevelUsingMateEntity(monsterEntity, mateEntity);
                });
                if (toFocus == null)
                {
                    return;
                }

                AddEntityToTargets(monsterEntity, toFocus);
                return;
            }

            if (monsterEntity.ModeIsHpTriggered)
            {
                TryActivateMode(monsterEntity);
            }

            if (isModeActive != monsterEntity.ModeIsActive)
            {
                return;
            }

            if (skillInfo.Vnum != 0 && skillInfo.TargetType == TargetType.Self)
            {
                skillTarget = monsterEntity;
            }

            Position positionAfterDash = default;
            if (skillInfo.AttackType == AttackType.Dash && !monsterEntity.MapInstance.IsBlockedZone(monsterEntity.Target.Position.X, monsterEntity.Target.Position.Y)
                && monsterEntity.Position.IsInRange(monsterEntity.Target.Position, skillInfo.Range + 2))
            {
                positionAfterDash = monsterEntity.Target.Position;
            }

            monsterEntity.RemoveEntityMp((short)skillInfo.ManaCost, skillToUse?.Skill);

            int castTick = (2 + monsterEntity.BasicCastTime) * 100;
            DateTime castFinish = monsterEntity.GenerateSkillCastTime(skillInfo) + TimeSpan.FromMilliseconds(castTick < 200 ? 200 : castTick);

            monsterEntity.LastSkill = castFinish + TimeSpan.FromMilliseconds(monsterEntity.ApplyCooldownReduction(skillInfo) * 100);
            monsterEntity.LastAttackedEntity = (skillTarget.Type, skillTarget.Id);

            short castTime = monsterEntity.GenerateSkillCastTimeNumber(skillInfo);

            monsterEntity.NextTick = castFinish + TimeSpan.FromMilliseconds(monsterEntity.BasicCastTime * 100 + castTime);
            monsterEntity.EmitEvent(new BattleExecuteSkillEvent(monsterEntity, skillTarget, skillInfo, castFinish, positionAfterDash));

            if (monster.SkillToUse != skillInfo.Vnum)
            {
                return;
            }

            monster.ForceUseSkill = false;
            monster.SkillToUse = null;
        }

        private void TryMoveToFirstPosition(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (!monsterEntity.IsStillAlive)
            {
                return;
            }

            if (!monsterEntity.IsAlive())
            {
                return;
            }

            if (!monsterEntity.CanWalk)
            {
                return;
            }

            if (!MovementPreChecks(monsterEntity))
            {
                return;
            }

            if (monsterEntity.ReturningToFirstPosition)
            {
                return;
            }

            if (monsterEntity.Target != null)
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
            }

            monsterEntity.ReturningToFirstPosition = true;
            ProcessMovement(monsterEntity, monsterEntity.FirstX, monsterEntity.FirstY);
        }

        private void ApproachTarget(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (monsterEntity.Target == null)
            {
                return;
            }

            if (!monsterEntity.CanSeeInvisible)
            {
                // if target is player and he become invisible
                if (monsterEntity.Target.IsInvisible())
                {
                    RemoveTarget(monsterEntity, monsterEntity.Target, true);
                    return;
                }
            }

            if (monsterEntity.Target.IsMonsterAggroDisabled())
            {
                RemoveTarget(monsterEntity, monsterEntity.Target);
                return;
            }

            bool isInSeekRange = monsterEntity.Target.Position.IsInRange(monsterEntity.Position, monsterEntity.NoticeRange * 2);
            bool isInBigSeekRange = monsterEntity.Target.Position.IsInRange(monsterEntity.Position, monsterEntity.NoticeRange * 3);

            if (!isInBigSeekRange && date < monsterEntity.AttentionTime - TimeSpan.FromMinutes(1))
            {
                RemoveTarget(monsterEntity, monsterEntity.Target, true);
                if (monsterEntity.Targets.Count != 0)
                {
                    return;
                }

                ForgetAll(monsterEntity, date, false);
                return;
            }

            if (!isInSeekRange && date < monsterEntity.AttentionTime - TimeSpan.FromMinutes(1))
            {
                RemoveTarget(monsterEntity, monsterEntity.Target, true);
                if (monsterEntity.Targets.Count != 0)
                {
                    return;
                }

                ForgetAll(monsterEntity, date, false);
                return;
            }

            if (date > monsterEntity.AttentionTime)
            {
                RemoveTarget(monsterEntity, monsterEntity.Target, true);
                if (monsterEntity.Targets.Count != 0)
                {
                    return;
                }

                ForgetAll(monsterEntity, date, false);
                return;
            }

            if (!MovementPreChecks(monsterEntity))
            {
                TryMoveToFirstPosition(monsterEntity, date);
                return;
            }

            short targetX = monsterEntity.Target.PositionX;
            short targetY = monsterEntity.Target.PositionY;

            if (monsterEntity.IsRunningAway)
            {
                monsterEntity.AttentionTime = date + TimeSpan.FromSeconds(15);

                short newX;
                short newY;
                if (monsterEntity.PositionX == monsterEntity.Target.PositionX && monsterEntity.PositionY == monsterEntity.Target.PositionY)
                {
                    newX = 0;
                    newY = 0;
                }
                else
                {
                    newX = (short)(monsterEntity.PositionX + (monsterEntity.PositionX - targetX) * 50);
                    newY = (short)(monsterEntity.PositionY + (monsterEntity.PositionY - targetY) * 50);
                }

                targetX = newX;
                targetY = newY;
            }

            ProcessMovement(monsterEntity, targetX, targetY);
        }

        private void ShowEffect(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (monsterEntity.IsBonus && (date - monsterEntity.LastBonusEffectTime).TotalSeconds >= 3)
            {
                monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateEffectPacket((int)EffectType.TsBonus), new RangeBroadcast(monsterEntity.PositionX, monsterEntity.PositionY));
                monsterEntity.LastBonusEffectTime = date;
            }

            if ((date - monsterEntity.LastEffect).TotalSeconds < 5)
            {
                return;
            }

            if (monsterEntity.PermanentEffect != 0)
            {
                monsterEntity.BroadcastEffectInRange(monsterEntity.PermanentEffect);
            }

            if (monsterEntity.IsTarget)
            {
                monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateEffectPacket((int)EffectType.TsTarget));
            }

            monsterEntity.LastEffect = date;
        }

        private void WalkAround(IMonsterEntity entity, in DateTime date)
        {
            if (!MovementPreChecks(entity))
            {
                return;
            }

            if (entity.ShouldFindNewTarget)
            {
                FindTarget(entity, date);
                entity.ShouldFindNewTarget = false;

                if (entity.Target != null)
                {
                    return;
                }

                TryMoveToFirstPosition(entity, date);
                ForgetAll(entity, date);
                entity.NextTick += TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
                return;
            }

            if (entity.GoToBossPosition.HasValue && entity.GoToBossPosition.Value != default)
            {
                short x = entity.GoToBossPosition.Value.X;
                short y = entity.GoToBossPosition.Value.Y;

                if (entity.IsMonsterAggroDisabled(x, y))
                {
                    return;
                }

                ProcessMovement(entity, x, y);

                if (entity.GoToBossPosition != null && entity.GoToBossPosition.Value == entity.Position)
                {
                    entity.GoToBossPosition = null;
                }

                return;
            }

            if (entity.Waypoints != null)
            {
                if (entity.LastWayPoint > date)
                {
                    return;
                }

                Waypoint currentState = entity.Waypoints.TryGetValue(entity.CurrentWayPoint, out Waypoint waypoint) ? waypoint : null;
                if (currentState == null)
                {
                    entity.CurrentWayPoint = 0;
                    return;
                }

                if (entity.PositionX == currentState.X && entity.PositionY == currentState.Y)
                {
                    entity.CurrentWayPoint++;
                    entity.LastWayPoint = date.AddMilliseconds(currentState.WaitTime);
                    return;
                }

                short stateMapX = currentState.X;
                short stateMapY = currentState.Y;

                if (entity.MapInstance.IsBlockedZone(stateMapX, stateMapY))
                {
                    return;
                }

                if (entity.IsMonsterAggroDisabled(stateMapX, stateMapY))
                {
                    return;
                }

                ProcessMovement(entity, stateMapX, stateMapY);
                return;
            }

            short mapX = entity.FirstX;
            short mapY = entity.FirstY;

            if (!entity.MapInstance.GetFreePosition(_randomGenerator, ref mapX, ref mapY, (byte)_randomGenerator.RandomNumber(0, 5), (byte)_randomGenerator.RandomNumber(0, 5)))
            {
                return;
            }

            if (entity.MapInstance.IsBlockedZone(mapX, mapY))
            {
                return;
            }

            if (entity.IsMonsterAggroDisabled(mapX, mapY))
            {
                return;
            }

            ProcessMovement(entity, mapX, mapY);
        }

        /// <summary>
        ///     Returns True if you can move and False if you can't
        /// </summary>
        /// <param name="monsterEntity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool MovementPreChecks(IMonsterEntity monsterEntity)
        {
            if (monsterEntity.BCardComponent.HasBCard(BCardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
            {
                return false;
            }

            return monsterEntity.IsMoving && monsterEntity.IsStillAlive && monsterEntity.CanWalk && monsterEntity.Speed > 0;
        }

        private void ProcessRespawnLogic(IMonsterEntity monsterEntity, in DateTime date)
        {
            if (!monsterEntity.ShouldRespawn)
            {
                ForgetAll(monsterEntity, date);
                monsterEntity.BroadcastDie();
                monsterEntity.MapInstance.RemoveMonster(monsterEntity);
                return;
            }

            TimeSpan effectiveRespawn = monsterEntity.BaseRespawnTime;

            if (date - monsterEntity.Death < effectiveRespawn)
            {
                return;
            }

            if (monsterEntity.ModeIsHpTriggered)
            {
                monsterEntity.ModeIsActive = false;
            }
            else
            {
                ActivateMode(monsterEntity);
            }

            if (monsterEntity.MonsterVNum is 2552 or 2553)
            {
                monsterEntity.MapInstance.Broadcast(x =>
                    x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.PVP_INSTANCE_MINI_BOSS_RESPAWNED, monsterEntity.Name), MsgMessageType.Middle));
            }

            ForgetAll(monsterEntity, date);
            monsterEntity.Target = null;
            monsterEntity.ModeDeathsSinceRespawn = _totalMonstersDeaths;
            monsterEntity.SpawnDate = date;
            monsterEntity.IsStillAlive = true;
            monsterEntity.ReturningToFirstPosition = false;
            monsterEntity.ShouldFindNewTarget = false;
            monsterEntity.OnFirstDamageReceive = true;
            monsterEntity.CancelCastingSkill();
            monsterEntity.ChargeComponent.ResetCharge();

            monsterEntity.Hp = monsterEntity.MaxHp;
            monsterEntity.Mp = monsterEntity.MaxMp;
            monsterEntity.NextTick = date + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
            monsterEntity.NextAttackReady = date;
            monsterEntity.ChangePosition(new Position(monsterEntity.FirstX, monsterEntity.FirstY));

            monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateIn(monsterEntity.MonsterRaceType != MonsterRaceType.Fixed));

            _eventPipeline.ProcessEventAsync(new MonsterRespawnedEvent
            {
                Monster = monsterEntity
            }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void ProcessMovement(IMonsterEntity entity, short mapX, short mapY)
        {
            switch (entity.ReturningToFirstPosition)
            {
                case true when entity.ReturnTimeOut > RETURN_TIME_OUT:
                    return;
                case false:
                    entity.ReturnTimeOut = 0;
                    break;
            }

            int speed = entity.Target != null || entity.ReturningToFirstPosition ? entity.Speed + entity.Speed / 2 : entity.Speed;
            
            double speedIndexDefault = Math.Ceiling(speed * 0.4f);
            float speedIndex = (float)(speedIndexDefault < 1 ? 1 : speedIndexDefault);

            Position position = _pathFinder.FindPath(entity.Position, new Position(mapX, mapY), speedIndex,
                entity.MapInstance.Grid, entity.MapInstance.Width, entity.MapInstance.Height, entity.ReturningToFirstPosition);

            Position pos = position;

            if (pos.X < 0 || pos.Y < 0)
            {
                pos = entity.Position;
            }

            if (pos == entity.Position && entity.ReturningToFirstPosition)
            {
                entity.ReturnTimeOut++;
            }
            
            if (entity.Target != null && pos == entity.Target.Position)
            {
                IReadOnlyList<Position> getRandomCell = entity.Target.Position.GetNeighbors(entity.MapInstance.Grid, entity.MapInstance.Width, entity.MapInstance.Height);
                if (getRandomCell.Count != 0)
                {
                    pos = getRandomCell[_randomGenerator.RandomNumber(0, getRandomCell.Count)];
                }

                if (entity.Target == null || entity.MapInstance == null)
                    return;
                if (pos == entity.Target.Position || entity.MapInstance.IsBlockedZone(pos.X, pos.Y))
                {
                    pos = position;
                }
            }

            entity.ChangePosition(pos);
            string packet = entity.GenerateMvPacket(speed);

            _mapInstance.Broadcast(packet);

            if (!entity.ReturningToFirstPosition)
            {
                return;
            }

            if (entity.Position.X != entity.FirstX || entity.Position.Y != entity.FirstY)
            {
                return;
            }

            entity.ReturningToFirstPosition = false;
        }

        private bool CheckTargetHeroLevelUsingMateEntity(IMonsterEntity monsterEntity, IMateEntity mateEntity)
        {
            if (!monsterEntity.IsSparringMonster)
            {
                return false;
            }

            int? targetMaxHeroLevel = _trainerSpecialistConfiguration.GetMaxHeroLevelTargetByMonsterVnum(monsterEntity.MonsterVNum);

            if (!targetMaxHeroLevel.HasValue)
            {
                return false;
            }

            MaxPetLevelConfiguration starsInfo = _petMaxLevelConfiguration.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

            return mateEntity.HeroLevel >= targetMaxHeroLevel.Value || !(mateEntity.HeroLevel < starsInfo?.MaxLevel);
        }

        private bool CheckTargetHeroLevel(IMonsterEntity monsterEntity, in DateTime date, bool isTickRefresh)
        {
            if (!monsterEntity.IsSparringMonster || !monsterEntity.Target.IsMate())
            {
                return false;
            }

            if (monsterEntity.Target is not IMateEntity mate)
            {
                return false;
            }

            int? targetMaxHeroLevel = _trainerSpecialistConfiguration.GetMaxHeroLevelTargetByMonsterVnum(monsterEntity.MonsterVNum);

            if (!targetMaxHeroLevel.HasValue)
            {
                return false;
            }

            MaxPetLevelConfiguration starsInfo = _petMaxLevelConfiguration.Configurations.FirstOrDefault(s => s.Stars == mate.Stars);

            return mate.HeroLevel >= targetMaxHeroLevel.Value || !(mate.HeroLevel < starsInfo?.MaxLevel);
        }
    }
}