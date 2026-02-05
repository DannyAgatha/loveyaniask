using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using PhoenixLib.Events;
using Plugin.CoreImpl.Pathfinding;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Core.Extensions;
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
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class NpcSystem : IMapSystem, INpcSystem
    {
        private const int TICK_DELAY_MILLISECONDS = 100;
        private const int RETURN_TIME_OUT = 5;

        private static readonly TimeSpan _refreshRate = TimeSpan.FromMilliseconds(TICK_DELAY_MILLISECONDS);

        private readonly BCardTickSystem _bCardTickSystem;
        private readonly IBuffFactory _buffFactory;
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameLanguageService _gameLanguage;

        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly IMapInstance _mapInstance;
        private readonly IMonsterTalkingConfig _monsterTalkingConfig;
        private readonly List<INpcEntity> _npcs = new();
        private readonly ConcurrentDictionary<long, INpcEntity> _npcsById = new();
        private readonly IPathFinder _pathFinder;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ConcurrentQueue<INpcEntity> _toAddNpcs = new();
        private readonly ConcurrentQueue<INpcEntity> _toRemoveNpcs = new();
        private DateTime _lastProcess = DateTime.MinValue;

        public NpcSystem(IBCardEffectHandlerContainer bcardHandlers, IMapInstance mapInstance,
            IAsyncEventPipeline eventPipeline, IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameLanguageService gameLanguage,
            IPathFinder pathFinder, IMonsterTalkingConfig monsterTalkingConfig)
        {
            _mapInstance = mapInstance;
            _eventPipeline = eventPipeline;
            _randomGenerator = randomGenerator;
            _buffFactory = buffFactory;
            _gameLanguage = gameLanguage;
            _pathFinder = pathFinder;
            _monsterTalkingConfig = monsterTalkingConfig;
            _bCardTickSystem = new BCardTickSystem(bcardHandlers, _randomGenerator, _buffFactory, _gameLanguage);
        }

        public void PutIdleState()
        {
            _bCardTickSystem.Clear();
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _npcs.Clear();
                _toRemoveNpcs.Clear();
                _toAddNpcs.Clear();
                _npcsById.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public string Name => nameof(NpcSystem);

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
                while (_toRemoveNpcs.TryDequeue(out INpcEntity toRemove))
                {
                    RemoveTarget(toRemove, date);
                    _npcsById.TryRemove(toRemove.Id, out _);
                    _npcs.Remove(toRemove);
                }

                while (_toAddNpcs.TryDequeue(out INpcEntity toAdd))
                {
                    _npcsById[toAdd.Id] = toAdd;
                    _npcs.Add(toAdd);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            foreach (INpcEntity monster in _npcs)
            {
                Update(date, monster, isTickRefresh);
            }
        }

        public void NpcRefreshTarget(INpcEntity npcEntity, IBattleEntity target)
        {
            if (npcEntity.Damagers.Contains(target))
            {
                return;
            }

            if (!npcEntity.CanSeeInvisible && target.IsInvisible())
            {
                return;
            }

            npcEntity.NextTick -= TimeSpan.FromMilliseconds(800);
            AggroLogic(npcEntity, target);
        }

        public IReadOnlyList<INpcEntity> GetAliveNpcs()
        {
            _lock.EnterReadLock();
            try
            {
                return _npcs.FindAll(s => s != null && s.IsAlive() && s.IsStillAlive && s.CanAttack && s.MonsterRaceType != MonsterRaceType.Other && s.MonsterRaceType != MonsterRaceType.Fixed);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<INpcEntity> GetAliveNpcs(Func<INpcEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _npcs.FindAll(s => s != null && s.IsAlive() && s.IsStillAlive && s.CanAttack
                    && s.MonsterRaceType != MonsterRaceType.Other && s.MonsterRaceType != MonsterRaceType.Fixed && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        public IReadOnlyList<INpcEntity> GetPassiveNpcs()
        {
            _lock.EnterReadLock();
            try
            {
                return _npcs.FindAll(s => s != null && (s.CanAttack == false || s.MonsterRaceType is MonsterRaceType.Other or MonsterRaceType.Fixed));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance, Func<INpcEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _npcs.FindAll(s =>
                    s != null && s.IsAlive() && s.IsStillAlive && s.CanAttack
                    && s.MonsterRaceType != MonsterRaceType.Other
                    && s.MonsterRaceType != MonsterRaceType.Fixed
                    && pos.IsInAoeZone(s.Position, distance)
                    && (predicate == null || predicate(s))
                );
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance) => GetAliveNpcsInRange(pos, distance, null);

        public IReadOnlyList<INpcEntity> GetClosestNpcsInRange(Position pos, short distance)
        {
            _lock.EnterReadLock();
            try
            {
                List<INpcEntity> toReturn = _npcs.FindAll(s =>
                    s != null && s.IsAlive() && s.IsStillAlive && s.CanAttack
                    && s.MonsterRaceType != MonsterRaceType.Other
                    && s.MonsterRaceType != MonsterRaceType.Fixed
                    && pos.IsInAoeZone(s.Position, distance));
                toReturn.Sort((prev, next) => prev.Position.GetDistance(pos) - next.Position.GetDistance(pos));

                return toReturn;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public INpcEntity GetNpcById(long id) => _npcsById.GetOrDefault(id);

        public void AddNpc(INpcEntity entity)
        {
            _toAddNpcs.Enqueue(entity);
        }

        public void RemoveNpc(INpcEntity entity)
        {
            _toRemoveNpcs.Enqueue(entity);
        }

        private void Update(in DateTime date, INpcEntity npcEntity, bool isTickRefresh)
        {
            if (!npcEntity.IsStillAlive)
            {
                ProcessRespawnLogic(npcEntity, date);
                return;
            }

            if (npcEntity.SpawnDate.AddMilliseconds(500) > date)
            {
                return;
            }

            if (!npcEntity.IsAlive())
            {
                return;
            }

            ShowEffect(npcEntity, date);
            _bCardTickSystem.ProcessUpdate(npcEntity, date);
            ProcessRecurrentLifeDecrease(npcEntity, date);
            ProcessCollection(npcEntity, date);
            TryHealInTimeSpace(npcEntity, date);

            if (_mapInstance.AIDisabled)
            {
                return;
            }

            if (npcEntity.CharacterPartnerId.HasValue)
            {
                return;
            }

            if (npcEntity.IsCastingSkill)
            {
                IBattleEntity entity = npcEntity.MapInstance.GetBattleEntity(npcEntity.LastAttackedEntity.Item1, npcEntity.LastAttackedEntity.Item2);
                if (entity == null)
                {
                    npcEntity.CancelCastingSkill();
                }

                return;
            }

            if (npcEntity.NextTick > date)
            {
                return;
            }

            FindCharacterAsPartner(npcEntity);
            RefreshTarget(npcEntity, date);
            TryRunAway(npcEntity, date);
            TryTalk(npcEntity);

            if (npcEntity.IsApproachingTarget)
            {
                npcEntity.IsApproachingTarget = false;
                ApproachTarget(npcEntity, date);
                npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(1000);
                return;
            }

            if (npcEntity.FindNewPositionAroundTarget)
            {
                npcEntity.FindNewPositionAroundTarget = false;
                npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);

                if (npcEntity.Target == null)
                {
                    return;
                }

                short randomX = (short)(npcEntity.Target.Position.X + _randomGenerator.RandomNumber(-1, 2));
                short randomY = (short)(npcEntity.Target.Position.Y + _randomGenerator.RandomNumber(-1, 2));

                if (randomX == npcEntity.Position.X && randomY == npcEntity.Position.Y)
                {
                    return;
                }

                if (randomX == npcEntity.Target.Position.X && randomY == npcEntity.Target.PositionY)
                {
                    npcEntity.FindNewPositionAroundTarget = true;
                    return;
                }

                if (!MovementPreChecks(npcEntity))
                {
                    return;
                }

                if (npcEntity.MapInstance.IsBlockedZone(randomX, randomY))
                {
                    return;
                }

                ProcessMovement(npcEntity, randomX, randomY);
                return;
            }

            if (npcEntity.Target == null)
            {
                int random = _randomGenerator.RandomNumber();
                bool move = random <= 60;

                if (move || npcEntity.ReturningToFirstPosition)
                {
                    WalkAround(npcEntity, date);
                }

                npcEntity.NextTick = (isTickRefresh ? date + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000)) : npcEntity.NextTick) + TimeSpan.FromMilliseconds(1000);
                return;
            }

            TryFight(date, npcEntity, isTickRefresh);
        }

        private void TryRunAway(INpcEntity npcEntity, in DateTime date)
        {
            if (!npcEntity.IsRunningAway)
            {
                return;
            }

            ApproachTarget(npcEntity, date);
            npcEntity.NextTick += TimeSpan.FromMilliseconds(1000);
        }

        private void TryTalk(INpcEntity npcEntity)
        {
            if (!_monsterTalkingConfig.HasPossibleMessages(npcEntity.MonsterVNum))
            {
                return;
            }

            if (_randomGenerator.RandomNumber() > 5)
            {
                return;
            }

            IReadOnlyList<string> messages = _monsterTalkingConfig.PossibleMessage(npcEntity.MonsterVNum);
            if (messages == null)
            {
                return;
            }

            if (messages.Count < 1)
            {
                return;
            }

            string message = messages[_randomGenerator.RandomNumber(messages.Count)];
            
            npcEntity.MapInstance.Broadcast(x => npcEntity.GenerateNpcMessage(message),
                new RangeBroadcast(npcEntity.PositionX, npcEntity.PositionY, 30));
        }

        private void TryHealInTimeSpace(INpcEntity npcEntity, in DateTime date)
        {
            if (npcEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
            {
                return;
            }

            if (!npcEntity.IsAlive())
            {
                return;
            }

            if (!npcEntity.IsProtected)
            {
                return;
            }

            if (npcEntity.LastTimeSpaceHeal.AddSeconds(2) > date)
            {
                return;
            }

            npcEntity.LastTimeSpaceHeal = date;

            int hpToHeal = (int)(npcEntity.MaxHp * 0.01);
            int mpToHeal = (int)(npcEntity.MaxMp * 0.01);

            if (npcEntity.Hp + hpToHeal < npcEntity.MaxHp)
            {
                npcEntity.Hp += hpToHeal;
            }
            else
            {
                npcEntity.Hp = npcEntity.MaxHp;
            }

            if (npcEntity.Mp + mpToHeal < npcEntity.MaxMp)
            {
                npcEntity.Mp += mpToHeal;
            }
            else
            {
                npcEntity.Mp = npcEntity.MaxMp;
            }

            foreach (IClientSession session in npcEntity.MapInstance.Sessions)
            {
                session.SendPacket(npcEntity.GenerateStPacket());
            }
        }

        private void FindCharacterAsPartner(INpcEntity npcEntity)
        {
            if (npcEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
            {
                return;
            }

            if (!npcEntity.IsTimeSpaceMate)
            {
                return;
            }

            if (npcEntity.CharacterPartnerId.HasValue)
            {
                return;
            }

            IPlayerEntity getClosestCharacter = npcEntity.MapInstance.GetClosestCharactersInRange(npcEntity.Position, npcEntity.NoticeRange).FirstOrDefault();
            if (getClosestCharacter == null)
            {
                return;
            }

            if (getClosestCharacter.TimeSpaceComponent.Partners.Any(x => x.MonsterVNum == npcEntity.MonsterVNum))
            {
                return;
            }

            npcEntity.CharacterPartnerId = getClosestCharacter.Id;
            getClosestCharacter.TimeSpaceComponent.Partners.Add(npcEntity);
            getClosestCharacter.Session.SendNpcEffect(npcEntity, EffectType.PetPickUp);
            getClosestCharacter.Session.SendMateControl(npcEntity);
            getClosestCharacter.Session.SendCondMate(npcEntity);
        }

        private void ProcessCollection(INpcEntity entity, in DateTime date)
        {
            if (entity.MonsterRaceType != MonsterRaceType.Fixed && entity.MonsterRaceSubType != 7)
            {
                return;
            }

            if (entity.CurrentCollection >= entity.MaxTries)
            {
                return;
            }

            if (entity.LastCollection > date)
            {
                return;
            }

            entity.LastCollection = date.AddSeconds(entity.CollectionCooldown);
            entity.CurrentCollection++;
        }


        private void ProcessRecurrentLifeDecrease(INpcEntity entity, DateTime date)
        {
            if (entity.LastSpecialHpDecrease.AddSeconds(1) > date)
            {
                return;
            }

            entity.LastSpecialHpDecrease = date;
            if (entity.DisappearAfterSeconds)
            {
                int hpToDecrease = entity.MaxHp / (entity.MaxHp / 5);
                entity.Hp -= hpToDecrease;
                if (entity.Hp > 0)
                {
                    return;
                }

                _eventPipeline.ProcessEventAsync(new MapNpcGenerateDeathEvent(entity, null));
                return;
            }

            if (!entity.DisappearAfterSecondsMana)
            {
                return;
            }

            int toRemove = entity.MaxMp / (entity.MaxMp / 10);
            entity.Mp -= toRemove;
            if (entity.Mp > 0)
            {
                return;
            }

            _eventPipeline.ProcessEventAsync(new MapNpcGenerateDeathEvent(entity, null));
        }


        private void RemoveTarget(INpcEntity npcEntity, in DateTime time)
        {
            if (npcEntity.Target == null)
            {
                return;
            }

            npcEntity.Damagers.Remove(npcEntity.Target);
            npcEntity.Target = null;
            npcEntity.IsApproachingTarget = false;
            npcEntity.ShouldFindNewTarget = true;
        }

        private void RefreshTarget(INpcEntity npcEntity, in DateTime date)
        {
            // if monster is going back to his own position
            if (npcEntity.ReturningToFirstPosition)
            {
                return;
            }

            // if monster have a lot of damagers -> find nearest target
            if (npcEntity.Damagers.Count > 0)
            {
                IBattleEntity oldTarget = npcEntity.Target;
                IBattleEntity nearestTarget = npcEntity.Damagers.OrderBy(e => npcEntity.Position.GetDistance(e.Position))
                    .FirstOrDefault(x => npcEntity.IsEnemyWith(x) && x.IsAlive());

                if (oldTarget != null && nearestTarget != null && oldTarget.Id != nearestTarget.Id && oldTarget.Type != nearestTarget.Type)
                {
                    RemoveTarget(npcEntity, date);
                }

                npcEntity.Target = nearestTarget;
            }

            // if someone attack monster
            if (npcEntity.Target != null)
            {
                // if target is on diffrent map 
                // if target is dead
                if (npcEntity.Target.MapInstance?.Id != npcEntity.MapInstance?.Id || !npcEntity.Target.IsAlive())
                {
                    RemoveTarget(npcEntity, date);
                    return;
                }
            }

            // if monster is agressive, find target || looking for whether or not his companions were attacked
            if (npcEntity.IsHostile || npcEntity.GroupAttack != (int)GroupAttackType.None)
            {
                FindTarget(npcEntity, date);
                return;
            }

            // if nobody attack him
            if (npcEntity.Target == null && npcEntity.Damagers.Count == 0)
            {
                return;
            }

            if (npcEntity.Target != null)
            {
                return;
            }

            FindTarget(npcEntity, date);
        }

        private void FindTarget(INpcEntity npcEntity, in DateTime time)
        {
            IBattleEntity target = null;

            if (npcEntity.IsHostile)
            {
                target = HostileFinding(npcEntity);
            }

            if (target == null)
            {
                return;
            }

            NpcRefreshTarget(npcEntity, target);
        }

        private IBattleEntity HostileFinding(INpcEntity npcEntity)
        {
            short noticeRange = npcEntity.NoticeRange;
            IEnumerable<IBattleEntity> targets = npcEntity.MapInstance.GetAliveMonstersInRange(npcEntity.Position, noticeRange);
            return BasicTargetChecks(npcEntity, targets);
        }

        private IBattleEntity BasicTargetChecks(INpcEntity npcEntity, IEnumerable<IBattleEntity> targets)
        {
            targets = targets.Where(e =>
            {
                if (!npcEntity.CanHit(e) || !e.IsAlive() || npcEntity.IsAllyWith(e))
                {
                    return false;
                }

                return npcEntity.CanSeeInvisible || !e.IsInvisible();
            });

            return targets.OrderBy(npcEntity.GetDistance).FirstOrDefault();
        }

        private void AggroLogic(INpcEntity npcEntity, IBattleEntity target)
        {
            npcEntity.Target = target;
            npcEntity.Damagers.Add(target);
        }

        private void ForgetAll(INpcEntity npcEntity, in DateTime time)
        {
            if (npcEntity.Target != null)
            {
                RemoveTarget(npcEntity, time);
            }

            npcEntity.LastSkill = DateTime.MinValue;
            npcEntity.Damagers.Clear();
        }

        private void TryFight(in DateTime date, INpcEntity npcEntity, bool isTickRefresh)
        {
            if (npcEntity == null)
            {
                return;
            }

            if (!npcEntity.IsAlive())
            {
                return;
            }

            if (!npcEntity.IsStillAlive)
            {
                return;
            }

            if (npcEntity.Target == null)
            {
                return;
            }

            if (npcEntity.IsRunningAway)
            {
                return;
            }

            if (!npcEntity.Target.IsAlive())
            {
                RemoveTarget(npcEntity, date);
                npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            if (!npcEntity.CanSeeInvisible)
            {
                // if target is player and he become invisible
                if (npcEntity.Target.IsInvisible())
                {
                    RemoveTarget(npcEntity, date);
                    npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                    return;
                }
            }

            if (!npcEntity.CanHit(npcEntity.Target))
            {
                RemoveTarget(npcEntity, date);
                npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            IReadOnlyList<INpcMonsterSkill> monsterSkills = npcEntity.NotBasicSkills;

            IBattleEntitySkill dashSkill = null;
            if (npcEntity.HasDash)
            {
                dashSkill = npcEntity.DashSkill;
                monsterSkills = npcEntity.SkillsWithoutDashSkill;
            }

            int count = monsterSkills.Count;
            IBattleEntitySkill getRandomSkill = count != 0 ? monsterSkills[_randomGenerator.RandomNumber(0, count)] : null;

            bool randomSkill = _randomGenerator.RandomNumber() <= getRandomSkill?.Rate;

            // Find basic skill that replace ZSKILL
            IBattleEntitySkill replacedBasic = npcEntity.ReplacedBasicSkill;
            IBattleEntitySkill skillToUse = randomSkill ? getRandomSkill : replacedBasic;
            SkillInfo skillInfo = skillToUse?.Skill.GetInfo(battleEntity: npcEntity) ?? npcEntity.BasicSkill;

            bool randomSkillCantBeUsed = skillToUse != null && !npcEntity.SkillCanBeUsed(skillToUse, date) && skillInfo.Vnum != 0 && skillToUse != replacedBasic;

            if (randomSkillCantBeUsed)
            {
                skillToUse = replacedBasic;
                skillInfo = skillToUse?.Skill.GetInfo(battleEntity: npcEntity) ?? npcEntity.BasicSkill;
            }

            short effectiveRange = skillInfo.Range == 0 || skillInfo.TargetType == TargetType.Self ? skillInfo.AoERange : skillInfo.Range;

            bool isInRange = npcEntity.IsInRange(npcEntity.Target.PositionX, npcEntity.Target.PositionY, (byte)effectiveRange);

            bool shouldWalk = skillInfo.TargetAffectedEntities !=
                TargetAffectedEntities.BuffForAllies; //skillInfo.TargetType == TargetType.Target && skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies ||
            //skillInfo.TargetType == TargetType.Self && skillInfo.HitType is TargetHitType.AlliesInAffectedAoE or TargetHitType.EnemiesInAffectedAoE;

            if (!isInRange && shouldWalk)
            {
                if (npcEntity.HasDash && dashSkill != null)
                {
                    skillToUse = dashSkill;
                    skillInfo = dashSkill.Skill.GetInfo();

                    effectiveRange = skillInfo.Range == 0 || skillInfo.TargetType == TargetType.Self ? skillInfo.AoERange : skillInfo.Range;
                    if (!npcEntity.SkillCanBeUsed(skillToUse, date) || !npcEntity.CanPerformAttack()
                        || !npcEntity.IsInRange(npcEntity.Target.PositionX, npcEntity.Target.PositionY, (byte)effectiveRange)
                        && skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies)
                    {
                        npcEntity.IsApproachingTarget = true;
                        npcEntity.NextTick = date;
                        return;
                    }

                    if (_randomGenerator.RandomNumber() > skillToUse.Rate)
                    {
                        npcEntity.IsApproachingTarget = true;
                        npcEntity.NextTick = date;
                        return;
                    }
                }
                else
                {
                    npcEntity.IsApproachingTarget = true;
                    npcEntity.NextTick = date;
                    return;
                }
            }

            npcEntity.LastSkill = date;

            if (npcEntity.NextAttackReady > date)
            {
                return;
            }

            if (getRandomSkill != null && getRandomSkill != replacedBasic && !randomSkill && npcEntity.SkillCanBeUsed(getRandomSkill, date))
            {
                npcEntity.SetSkillCooldown(getRandomSkill.Skill.GetInfo());
            }

            int random = _randomGenerator.RandomNumber();
            if (npcEntity.BasicHitChance == 0 || random >= npcEntity.BasicHitChance * 20)
            {
                if (skillToUse is null or INpcMonsterSkill { IsIgnoringHitChance: false })
                {
                    npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                    return;
                }
            }

            IBattleEntity skillTarget = npcEntity.Target;

            (int firstData, int secondData) cooldownToIncrease = (0, 0);
            (int firstData, int secondData) cooldownToDecrease = (0, 0);

            if (npcEntity.BCards.Any())
            {
                cooldownToIncrease = npcEntity.BCardComponent.GetAllBCardsInformation(BCardType.Mode, (byte)AdditionalTypes.Mode.AttackTimeIncreased, npcEntity.Level);
                cooldownToDecrease = npcEntity.BCardComponent.GetAllBCardsInformation(BCardType.Mode, (byte)AdditionalTypes.Mode.AttackTimeDecreased, npcEntity.Level);
            }

            int basicCooldown = npcEntity.BasicCooldown;

            if (cooldownToIncrease.firstData != 0)
            {
                basicCooldown += cooldownToIncrease.firstData;
            }

            if (cooldownToDecrease.firstData != 0)
            {
                basicCooldown -= cooldownToDecrease.firstData;
            }

            int tickToAdd = (2 + npcEntity.BasicCastTime + 2 * basicCooldown) * 100;
            tickToAdd = tickToAdd < 800 ? 800 : tickToAdd;
            npcEntity.NextAttackReady = date + TimeSpan.FromMilliseconds(tickToAdd);

            if (replacedBasic != null && skillInfo.Vnum == replacedBasic.Skill.Id && !npcEntity.SkillCanBeUsed(replacedBasic, date))
            {
                return;
            }

            if (!npcEntity.CanPerformAttack())
            {
                return;
            }

            if (!npcEntity.Target.IsAlive())
            {
                RemoveTarget(npcEntity, date);
                npcEntity.NextTick = (isTickRefresh ? date : npcEntity.NextTick) + TimeSpan.FromMilliseconds(200);
                return;
            }

            if (skillInfo.Vnum != 0 && skillInfo.TargetType == TargetType.Self)
            {
                skillTarget = npcEntity;
            }

            Position positionAfterDash = default;
            if (skillInfo.AttackType == AttackType.Dash && !npcEntity.MapInstance.IsBlockedZone(npcEntity.Target.Position.X, npcEntity.Target.Position.Y)
                && npcEntity.Position.IsInRange(npcEntity.Target.Position, skillInfo.Range + 2))
            {
                positionAfterDash = npcEntity.Target.Position;
            }

            npcEntity.RemoveEntityMp((short)skillInfo.ManaCost, skillToUse?.Skill);
            npcEntity.LastSkill = npcEntity.GenerateSkillCastTime(skillInfo) + TimeSpan.FromMilliseconds(npcEntity.ApplyCooldownReduction(skillInfo) * 100);
            npcEntity.LastAttackedEntity = (skillTarget.Type, skillTarget.Id);
            npcEntity.EmitEvent(new BattleExecuteSkillEvent(npcEntity, skillTarget, skillInfo, npcEntity.GenerateSkillCastTime(skillInfo), positionAfterDash));
        }

        private void TryMoveToFirstPosition(INpcEntity npcEntity, in DateTime date)
        {
            if (npcEntity.Target != null)
            {
                return;
            }

            if (!npcEntity.IsStillAlive)
            {
                return;
            }

            if (!npcEntity.IsAlive())
            {
                return;
            }

            if (!npcEntity.CanWalk)
            {
                return;
            }

            if (!MovementPreChecks(npcEntity))
            {
                return;
            }

            if (npcEntity.ReturningToFirstPosition)
            {
                return;
            }

            npcEntity.ReturningToFirstPosition = true;
            ProcessMovement(npcEntity, npcEntity.FirstX, npcEntity.FirstY);
        }

        private void ApproachTarget(INpcEntity npcEntity, DateTime date)
        {
            if (npcEntity.Target == null)
            {
                return;
            }

            if (!MovementPreChecks(npcEntity))
            {
                TryMoveToFirstPosition(npcEntity, date);
                return;
            }

            if (!npcEntity.CanSeeInvisible)
            {
                // if target is player and he become invisible
                if (npcEntity.Target.IsInvisible())
                {
                    RemoveTarget(npcEntity, date);
                    return;
                }
            }

            if (npcEntity.LastSkill != DateTime.MinValue && npcEntity.LastSkill.AddSeconds(15) <= date)
            {
                RemoveTarget(npcEntity, date);
                return;
            }

            short targetX = npcEntity.Target.PositionX;
            short targetY = npcEntity.Target.PositionY;

            if (npcEntity.IsRunningAway)
            {
                short newX;
                short newY;
                if (npcEntity.PositionX == npcEntity.Target.PositionX && npcEntity.PositionY == npcEntity.Target.PositionY)
                {
                    newX = 0;
                    newY = 0;
                }
                else
                {
                    newX = (short)(npcEntity.PositionX + (npcEntity.PositionX - targetX) * 50);
                    newY = (short)(npcEntity.PositionY + (npcEntity.PositionY - targetY) * 50);
                }

                targetX = newX;
                targetY = newY;
            }

            ProcessMovement(npcEntity, targetX, targetY);
        }

        private void ShowEffect(INpcEntity entity, DateTime date)
        {
            if ((date - entity.LastFrozenEffect).TotalSeconds >= 1 && entity.IsFrozenByGlacerus())
            {
                entity.LastFrozenEffect = date;
                entity.MapInstance.Broadcast(entity.GenerateEffectPacket(EffectType.Frozen));
            }
            
            if (entity.LastEffect.AddSeconds(5) > date)
            {
                return;
            }

            if (entity.IsTimeSpaceMate || entity.IsProtected)
            {
                entity.MapInstance.Broadcast(entity.GenerateEffectPacket(EffectType.TsMate), new RangeBroadcast(entity.PositionX, entity.PositionY));
            }

            if (entity.Effect > 0)
            {
                entity.MapInstance.Broadcast(entity.GenerateEffectPacket(entity.Effect), new RangeBroadcast(entity.PositionX, entity.PositionY));
            }

            if (entity.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle && entity.RainbowFlag != null)
            {
                RainbowBattleFlagTeamType teamFlag = entity.RainbowFlag.FlagTeamType;
                EffectType effectType = teamFlag switch
                {
                    RainbowBattleFlagTeamType.None => EffectType.NoneFlag,
                    RainbowBattleFlagTeamType.Red => EffectType.RedFlag,
                    RainbowBattleFlagTeamType.Blue => EffectType.BlueFlag,
                    _ => EffectType.NoneFlag
                };

                entity.MapInstance.Broadcast(entity.GenerateEffectPacket(effectType));
            }

            entity.LastEffect = date;
        }


        private void WalkAround(INpcEntity entity, in DateTime date)
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

            ProcessMovement(entity, mapX, mapY);
        }

        /// <summary>
        ///     Returns True if you can move and False if you can't
        /// </summary>
        /// <param name="npcEntity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool MovementPreChecks(INpcEntity npcEntity)
        {
            if (npcEntity.BCardComponent.HasBCard(BCardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
            {
                return false;
            }
            
            if (npcEntity.IsFrozenByGlacerus())
            {
                return false;
            }

            if (!npcEntity.IsStillAlive)
            {
                return false;
            }

            if (npcEntity.Speed <= 0)
            {
                return false;
            }

            if (npcEntity.IsProtected || npcEntity.IsTimeSpaceMate)
            {
                return true;
            }

            if (npcEntity.CanMove)
            {
                return true;
            }

            return npcEntity.IsMoving && npcEntity.CanWalk;
        }

        private void ProcessRespawnLogic(INpcEntity npcEntity, in DateTime date)
        {
            if (!npcEntity.ShouldRespawn)
            {
                npcEntity.MapInstance.Broadcast(npcEntity.GenerateOut());
                npcEntity.MapInstance.RemoveNpc(npcEntity);
                return;
            }

            if (!(date - npcEntity.Death >= npcEntity.BaseRespawnTime))
            {
                ForgetAll(npcEntity, date);
                return;
            }

            npcEntity.SpawnDate = date;
            npcEntity.IsStillAlive = true;
            npcEntity.ReturningToFirstPosition = false;
            npcEntity.Hp = npcEntity.MaxHp;
            npcEntity.Mp = npcEntity.MaxMp;
            npcEntity.NextTick = date + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
            npcEntity.NextAttackReady = date;
            npcEntity.CancelCastingSkill();
            npcEntity.ChangePosition(new Position(npcEntity.FirstX, npcEntity.FirstY));
            npcEntity.CurrentCollection = npcEntity.MaxTries;
            npcEntity.MapInstance.Broadcast(npcEntity.GenerateIn(npcEntity.MonsterRaceType != MonsterRaceType.Fixed));
        }

        private void ProcessMovement(INpcEntity entity, short mapX, short mapY)
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

                if (pos == entity.Target.Position || entity.MapInstance.IsBlockedZone(pos.X, pos.Y))
                {
                    pos = position;
                }
            }

            entity.ChangePosition(pos);
            string packet = entity.GenerateMvPacket(speed);
            entity.MapInstance.Broadcast(packet);

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
    }
}