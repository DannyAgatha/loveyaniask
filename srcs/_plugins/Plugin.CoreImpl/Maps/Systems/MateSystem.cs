using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class MateSystem : IMapSystem, IMateSystem
    {
        private const short LOYALTY_RECOVERED_PER_TICK = 100;
        private const short LOW_LOYALTY_EFFECT_IN_COMBAT = 100;
        private const short LOW_LOYALTY_EFFECT_FOR_CONTROL = 0;
        private const short LOW_LOYALTY_EFFECT_COST = 1;
        private const short LOW_LOYALTY_EFFECT_SECONDS_INTERVAL = 10;
        private const short LEVEL_WITHOUT_LOYALTY_PENALIZATION = 20;
        private static readonly TimeSpan RefreshRate = TimeSpan.FromMilliseconds(500);
        private readonly BCardTickSystem _bCardTickSystem;
        private readonly IBuffFactory _buffFactory;
        private readonly IGameLanguageService _gameLanguage;

        private readonly IGameLanguageService _languageService;

        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly IMapInstance _mapInstance;
        private readonly List<IMateEntity> _mates = new();
        private readonly ConcurrentDictionary<long, IMateEntity> _matesById = new();
        private readonly GameMinMaxConfiguration _minMaxConfiguration;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ISpPartnerConfiguration _spPartnerConfiguration;
        private readonly ConcurrentQueue<IMateEntity> _toAddMates = new();
        private readonly ConcurrentQueue<IMateEntity> _toRemoveMates = new();
        private DateTime _lastProcess = DateTime.MinValue;

        public MateSystem(IBCardEffectHandlerContainer bCardHandlers, IGameLanguageService languageService,
            GameMinMaxConfiguration minMaxConfiguration, IMapInstance mapInstance, IRandomGenerator randomGenerator,
            IBuffFactory buffFactory, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration)
        {
            _languageService = languageService;
            _minMaxConfiguration = minMaxConfiguration;
            _mapInstance = mapInstance;
            _randomGenerator = randomGenerator;
            _buffFactory = buffFactory;
            _gameLanguage = gameLanguage;
            _spPartnerConfiguration = spPartnerConfiguration;
            _bCardTickSystem = new BCardTickSystem(bCardHandlers, _randomGenerator, _buffFactory, _gameLanguage);
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
                _mates.Clear();
                _toAddMates.Clear();
                _toRemoveMates.Clear();
                _matesById.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public string Name => nameof(MateSystem);

        public void ProcessTick(DateTime date, bool isTickRefresh = false)
        {
            if (_lastProcess + RefreshRate > date)
            {
                return;
            }

            _lastProcess = date;

            _lock.EnterWriteLock();
            try
            {
                while (_toRemoveMates.TryDequeue(out IMateEntity toRemove))
                {
                    _matesById.TryRemove(toRemove.Id, out _);
                    _mates.Remove(toRemove);
                }

                while (_toAddMates.TryDequeue(out IMateEntity toAdd))
                {
                    if (_matesById.ContainsKey(toAdd.Id))
                    {
                        continue;
                    }

                    _matesById[toAdd.Id] = toAdd;
                    _mates.Add(toAdd);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            foreach (IMateEntity mate in _mates)
            {
                Update(date, mate);
            }
        }

        public IReadOnlyList<IMateEntity> GetAliveMates() => GetAliveMates(null);

        public IReadOnlyList<IMateEntity> GetAliveMates(Func<IMateEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _mates.FindAll(s => s != null && s.IsAlive() && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range) => GetAliveMatesInRange(position, range, null);

        public IReadOnlyList<IMateEntity> GetClosestMatesInRange(Position position, short range)
        {
            _lock.EnterReadLock();
            try
            {
                List<IMateEntity> toReturn = _mates.FindAll(s => s != null && s.IsAlive() && position.IsInAoeZone(s.Position, range));
                toReturn.Sort((prev, next) => prev.Position.GetDistance(position) - next.Position.GetDistance(position));

                return toReturn;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        public IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range, Func<IBattleEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _mates.FindAll(s => s != null && s.IsAlive() && position.IsInAoeZone(s.Position, (short)(range + s.CellSize)) && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IMateEntity GetMateById(long mateId) => _matesById.GetOrDefault(mateId);

        public void AddMate(IMateEntity entity)
        {
            _toAddMates.Enqueue(entity);
        }

        public void RemoveMate(IMateEntity entity)
        {
            _toRemoveMates.Enqueue(entity);
        }

        private void Update(DateTime date, IMateEntity entity)
        {
            ProcessSpawnByGuardian(date, entity);
            if (!entity.IsAlive())
            {
                ProcessRevivalEvents(date, entity);
                return;
            }

            if (entity.Owner?.Session == null)
            {
                return;
            }

            TryToHeal(date, entity);
            TryRecoveringLoyalty(date, entity);
            BroadcastMateEffect(date, entity);
            ProcessSpecialistCooldown(date, entity);
            _bCardTickSystem.ProcessUpdate(entity, date);

            TryShowLowLoyaltyEffect(date, entity);
        }

        private void ProcessSpawnByGuardian(in DateTime date, IMateEntity entity)
        {
            if (!entity.SpawnMateByGuardian.HasValue)
            {
                return;
            }

            if (entity.SpawnMateByGuardian.Value.AddSeconds(1.5) > date)
            {
                return;
            }

            IClientSession session = entity.Owner.Session;

            if (session.CurrentMapInstance == null)
            {
                return;
            }

            session.CurrentMapInstance.Broadcast(x => entity.GenerateOut());
            session.CurrentMapInstance.Broadcast(x => entity.GenerateOut());

            entity.SpawnMateByGuardian = null;
            entity.ChangePosition(entity.Owner.Position);
            entity.Hp = entity.SavedGuardianVnum is (int)ItemVnums.POWERFUL_NOSMATE_GUARDIAN_ANGEL or (int)ItemVnums.POWERFUL_PARTNER_GUARDIAN_ANGEL ? entity.MaxHp : entity.MaxHp / 2;
            entity.Mp = entity.SavedGuardianVnum is (int)ItemVnums.POWERFUL_NOSMATE_GUARDIAN_ANGEL or (int)ItemVnums.POWERFUL_PARTNER_GUARDIAN_ANGEL ? entity.MaxMp : entity.MaxMp / 2;
            entity.SavedGuardianVnum = null;
            session.PlayerEntity.MapInstance.Broadcast(x =>
            {
                bool isAnonymous = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) && x.PlayerEntity.Faction != session.PlayerEntity.Faction;
                string inPacket = entity.GenerateIn(_gameLanguage, x.UserLanguage, _spPartnerConfiguration, isAnonymous);
                return inPacket;
            });

            session.SendCondMate(entity);
            session.RefreshParty(_spPartnerConfiguration);
            session.SendPetInfo(entity, _gameLanguage);
            session.SendMateLife(entity);
        }

        private void BroadcastMateEffect(in DateTime date, IMateEntity entity)
        {
            if (!entity.CanPickUp)
            {
                return;
            }

            if (entity.LastEffect.AddSeconds(5) > date)
            {
                return;
            }

            if (entity.MapInstance == null)
            {
                return;
            }

            entity.LastEffect = date;
            entity.BroadcastEffectInRange(EffectType.PetPickupEnabled);
        }

        private void ProcessSpecialistCooldown(in DateTime date, IMateEntity entity)
        {
            if (entity.MateType == MateType.Pet)
            {
                return;
            }

            if (!entity.SpCooldownEnd.HasValue)
            {
                return;
            }

            if (entity.SpCooldownEnd.Value > date)
            {
                return;
            }

            entity.SpCooldownEnd = null;

            if (entity.Owner?.Session == null)
            {
                return;
            }

            IClientSession session = entity.Owner.Session;
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_CHATMESSAGE_TRANSFORM_DISAPPEAR, session.UserLanguage), ChatMessageColorType.Yellow);
            session.SendMateSpCooldown(entity, 0);
        }

        private static void ProcessRevivalEvents(in DateTime date, IMateEntity entity)
        {
            if (entity.Owner != null && entity.MapInstance.Id == entity.Owner.Miniland.Id && entity.Owner.MapInstance.MapInstanceType == MapInstanceType.Miniland)
            {
                entity.DisableRevival();
                entity.Owner.Session.EmitEvent(new MateReviveEvent(entity, true));
                entity.Hp = 1;
                entity.Mp = 1;
                return;
            }

            if (entity.RevivalDateTimeForExecution > date)
            {
                return;
            }

            entity.DisableRevival();
            entity.Owner.Session.EmitEvent(new MateReviveEvent(entity, false));
        }

        private static void TryToHeal(in DateTime date, IMateEntity entity)
        {
            if (entity.LastHealth.AddSeconds(entity.IsSitting ? 1.5 : 2) > date || entity.IsInCombat(date))
            {
                return;
            }

            entity.LastHealth = date;

            entity.Hp += entity.Hp + entity.HealthHpLoad() < entity.MaxHp ? entity.HealthHpLoad() : entity.MaxHp - entity.Hp;
            entity.Mp += entity.Mp + entity.HealthMpLoad() < entity.MaxMp ? entity.HealthMpLoad() : entity.MaxMp - entity.Mp;

            if (entity.Owner.GameStartDate.AddSeconds(5) <= DateTime.UtcNow)
            {
                if (entity.Hp > entity.MaxHp)
                {
                    entity.Hp = entity.MaxHp;
                }

                if (entity.Mp > entity.MaxMp)
                {
                    entity.Mp = entity.MaxMp;
                }
            }

            entity.Owner.Session.SendMateLife(entity);
        }

        private void TryShowLowLoyaltyEffect(in DateTime date, IMateEntity entity)
        {
            if (entity.LastLowLoyaltyEffect > date || entity.Loyalty > LOW_LOYALTY_EFFECT_IN_COMBAT)
            {
                return;
            }

            if (!entity.IsInCombat(date) && entity.Loyalty > LOW_LOYALTY_EFFECT_FOR_CONTROL && entity.Owner.Session.PlayerEntity.Level <= LEVEL_WITHOUT_LOYALTY_PENALIZATION)
            {
                return;
            }

            entity.Owner.Session.SendMateEffect(entity, EffectType.PetLoveBroke);
            entity.RemoveLoyalty(LOW_LOYALTY_EFFECT_COST, _minMaxConfiguration, _languageService);
            entity.LastLowLoyaltyEffect = date.AddSeconds(LOW_LOYALTY_EFFECT_SECONDS_INTERVAL);
        }

        private void TryRecoveringLoyalty(in DateTime date, IMateEntity entity)
        {
            if (entity.MapInstance?.Id != entity.Owner.Miniland?.Id)
            {
                return;
            }

            if (entity.LastLoyaltyRecover.AddSeconds(5) > DateTime.UtcNow)
            {
                return;
            }

            entity.LastLoyaltyRecover = date;
            entity.AddLoyalty(LOYALTY_RECOVERED_PER_TICK, _minMaxConfiguration, _languageService);
        }
    }
}