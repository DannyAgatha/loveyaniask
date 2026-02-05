// NosEmu
// 


using PhoenixLib.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Configurations.SetEffect;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Prestige;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.CoreImpl.Maps.Systems
{
    public class CharacterSystem : ICharacterSystem, IMapSystem
    {
        private static readonly TimeSpan ProcessSpInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan RefreshRate = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan ProcessBattlePassThings = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ProcessFoodInterval = TimeSpan.FromSeconds(6);
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly BCardTickSystem _bCardTickSystem;
        private readonly IBuffFactory _buffFactory;
        private readonly List<IPlayerEntity> _characters = new();
        private readonly ConcurrentDictionary<long, IPlayerEntity> _charactersById = new();
        private readonly IGameLanguageService _gameLanguage;
        private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
        private readonly SerializableGameServer _serializableGameServer;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly ILandOfDeathManager _landOfDeathManager;

        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly IMapInstance _mapInstance;
        private readonly IMeditationManager _meditationManager;
        private readonly SkillCooldownSystem _skillCooldownSystem;
        private readonly ISkillsManager _skillsManager;
        private readonly SnackFoodSystem _snackFoodSystem;
        private readonly ISpyOutManager _spyOutManager;
        private readonly ConcurrentQueue<IPlayerEntity> _toAddPlayers = new();
        private readonly ConcurrentQueue<IPlayerEntity> _toRemovePlayers = new();
        private readonly IRandomGenerator _randomGenerator;
        private readonly FamilyLevelBuffConfiguration _familyLevelBuffConfiguration;
        private readonly PrestigeConfiguration _prestigeConfiguration;
        private DateTime _lastProcess;
        private DateTime _lastClockPacket;
        public CharacterSystem(IBCardEffectHandlerContainer bcardHandlers, IBuffFactory buffFactory, IMeditationManager meditationManager, IAsyncEventPipeline asyncEventPipeline,
            IMapInstance mapInstance, ISpyOutManager spyOutManager, IRandomGenerator randomGenerator, ISkillsManager skillsManager,
            GameMinMaxConfiguration gameMinMaxConfiguration, IGameLanguageService gameLanguage, SerializableGameServer serializableGameServer, 
            IGameItemInstanceFactory gameItemInstanceFactory, IEvtbConfiguration evtbConfiguration, ILandOfDeathManager landOfDeathManager, FamilyLevelBuffConfiguration familyLevelBuffConfiguration, PrestigeConfiguration prestigeConfiguration)
        {
            _buffFactory = buffFactory;
            _meditationManager = meditationManager;
            _asyncEventPipeline = asyncEventPipeline;
            _mapInstance = mapInstance;
            _spyOutManager = spyOutManager;
            _randomGenerator = randomGenerator;
            _skillsManager = skillsManager;
            _gameLanguage = gameLanguage;
            _bCardEffectHandlerContainer = bcardHandlers;
            _snackFoodSystem = new SnackFoodSystem(gameMinMaxConfiguration, evtbConfiguration);
            _bCardTickSystem = new BCardTickSystem(bcardHandlers, randomGenerator, _buffFactory, _gameLanguage);
            _lastProcess = DateTime.MinValue;
            _skillCooldownSystem = new SkillCooldownSystem();
            _serializableGameServer = serializableGameServer;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _landOfDeathManager = landOfDeathManager;
            _familyLevelBuffConfiguration = familyLevelBuffConfiguration;
            _prestigeConfiguration = prestigeConfiguration;
        }

        public IPlayerEntity GetCharacterById(long id) => _charactersById.GetOrDefault(id);

        public IReadOnlyList<IPlayerEntity> GetCharacters()
        {
            _lock.EnterReadLock();
            try
            {
                return _characters.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IPlayerEntity> GetCharacters(Func<IPlayerEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _characters.FindAll(s => s != null && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        public IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position position, short range, Func<IPlayerEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _characters.FindAll(s => s != null && position.IsInAoeZone(s.Position, range) && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position pos, short distance) => GetCharactersInRange(pos, distance, null);

        public IReadOnlyList<IPlayerEntity> GetClosestCharactersInRange(Position pos, short distance)
        {
            _lock.EnterReadLock();
            try
            {
                List<IPlayerEntity> toReturn = _characters.FindAll(s => s != null && s.IsAlive() && pos.IsInAoeZone(s.Position, distance));
                toReturn.Sort((prev, next) => prev.Position.GetDistance(pos) - next.Position.GetDistance(pos));

                return toReturn;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IPlayerEntity> GetAliveCharacters() => GetCharacters(null);

        public IReadOnlyList<IPlayerEntity> GetAliveCharacters(Func<IPlayerEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _characters.FindAll(s => s != null && s.IsAlive() && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        public IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position position, short range, Func<IPlayerEntity, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _characters.FindAll(s => s != null && s.IsAlive() && position.IsInAoeZone(s.Position, range) && (predicate == null || predicate(s)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position pos, short distance) => GetCharactersInRange(pos, distance, null);

        public void AddCharacter(IPlayerEntity character)
        {
            _toAddPlayers.Enqueue(character);
        }

        public void RemoveCharacter(IPlayerEntity entity)
        {
            _toRemovePlayers.Enqueue(entity);
        }

        public string Name => nameof(CharacterSystem);

        public void ProcessTick(DateTime date, bool isTickRefresh = false)
        {
            if (_lastProcess + RefreshRate > date)
            {
                return;
            }

            _lastProcess = date;
            Update(date);
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
                _charactersById.Clear();
                _characters.Clear();
                _toAddPlayers.Clear();
                _toRemovePlayers.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void Update(DateTime date)
        {
            _lock.EnterWriteLock();
            try
            {
                while (_toRemovePlayers.TryDequeue(out IPlayerEntity player))
                {
                    RemovePrivateCharacter(player);
                }

                while (_toAddPlayers.TryDequeue(out IPlayerEntity player))
                {
                    AddPrivateCharacter(player);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            foreach (IPlayerEntity character in _characters)
            {
                Update(date, character);
            }
            
            if(_lastClockPacket.AddSeconds(1) < date)
            {
                ProcessClockPacket(date);
                _lastClockPacket = date;
            }
        }
        
        private void ProcessClockPacket(in DateTime date)
        {
            string timeString = date.ToString("HHmmss");
            int dayOfWeek = (int)date.DayOfWeek;
            dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
            string packet = $"server_time {timeString} {dayOfWeek}";

            foreach (IPlayerEntity character in _characters)
            {
                character.Session.SendPacket(packet);
            }
        }
        private void ProcessTartHapendamBuffs(IPlayerEntity player)
        {
            var buffs = new (short Buff, bool Condition)[]
            {
                ((short)BuffVnums.TART_HAPENDAM_NO_HERO, player.Level < 85),
                ((short)BuffVnums.TART_HAPENDAM_HERO, player.HeroLevel is > 0 and < 30)
            };

            foreach ((short buff, bool condition) in buffs)
            {
                bool hasBuff = player.HasBuff(buff);

                switch (hasBuff)
                {
                    case false when condition:
                        player.AddBuffAsync(_buffFactory.CreateBuff((short)buff, player))
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case true when !condition:
                        player.RemoveBuffAsync((short)buff)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    default:
                        break;
                }
            }
        }
        
        
        
        private void ProcessPrestigeBossTimeWarnings(DateTime now, IPlayerEntity character)
        {
            IMapInstance mapInstance = character.MapInstance;
            if (mapInstance is not { MapInstanceType: MapInstanceType.PrestigeInstance })
            {
                return;
            }

            if (!PrestigeInstanceManager.PrestigeInstances.TryGetValue(mapInstance, out PrestigeInstance prestigeInstance))
            {
                return;
            }

            TimeSpan elapsed = now - prestigeInstance.CreationTime;
            TimeSpan remaining = prestigeInstance.TimeLimit - elapsed;
            int remainingSec = (int)Math.Ceiling(remaining.TotalSeconds);

            foreach (WarningMilestoneConfig milestone in prestigeInstance.WarningMilestones)
            {
                if (remainingSec > milestone.Seconds || prestigeInstance.WarnedMilestones.Contains(milestone.Seconds) || remainingSec <= 0)
                {
                    continue;
                }
                
                string msg = character.Session.GetLanguageFormat(milestone.Key);
                character.Session.SendInfo(msg);
                prestigeInstance.WarnedMilestones.Add(milestone.Seconds);
            }

            if (remainingSec > 0 || prestigeInstance.WarnedMilestones.Contains(-1))
            {
                return;
            }

            character.Session.ChangeToLastBaseMap();
            character.Session.SendInfo(character.Session.GetLanguage(GameDialogKey.PRESTIGE_FINAL_CHALLENGE_TIME_UP));
            prestigeInstance.WarnedMilestones.Add(-1);
        }
        
        private void ProcessEnergyBar(DateTime date, IPlayerEntity character)
        {
            if (character.HasBuff(BuffVnums.IMPROVED_EXCESS_FUEL))
            {
                if (character.LastEnergy.AddSeconds(4) <= date)
                {
                    character.LastEnergy = date;
                    character.UpdateEnergyBar(-4).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            
            switch (character.Morph)
            {
                case (byte)MorphType.HolyMage:
                case (byte)MorphType.WaterfallBerserker:
                case (byte)MorphType.DragonKnight:
                case (byte)MorphType.Blaster:
                case (byte)MorphType.HydraulicFist:
                case (byte)MorphType.MasterWolf:
                    if (character.EnergyBar == 0)
                    {
                        return;
                    }

                    if (character.LastDefence.AddSeconds(15) >= date || character.LastAttack.AddSeconds(15) >= date
                        || character.LastEnergyRefill.AddSeconds(15) >= date)
                    {
                        return;
                    }

                    if (character.LastEnergy.AddSeconds(1) >= date)
                    {
                        return;
                    }

                    character.UpdateEnergyBar(-5).ConfigureAwait(false).GetAwaiter().GetResult();
                    character.LastEnergy = date;
                    break;
                case (byte)MorphType.Gravity:
                    if (character.EnergyBar == 0 && character.SecondEnergyBar == 0)
                    {
                        return;
                    }

                    if (character.LastDefence.AddSeconds(15) >= date || character.LastAttack.AddSeconds(15) >= date
                        || character.LastEnergyRefill.AddSeconds(15) >= date)
                    {
                        return;
                    }

                    if (character.LastEnergy.AddSeconds(1) >= date)
                    {
                        return;
                    }

                    character.UpdateBothEnergyBars(-5, -5).ConfigureAwait(false).GetAwaiter().GetResult();
                    character.LastEnergy = date;
                    break;
                
                case (byte)MorphType.StoneBreaker:
                case (byte)MorphType.FireStorm:
                case (byte)MorphType.FogHunter:
                    if (character.LastDefence.AddSeconds(15) >= date || character.LastAttack.AddSeconds(15) >= date || character.LastTokenEnergyRefill.AddSeconds(15) >= date)
                    {
                        return;
                    }

                    if (character.LastTokenEnergy.AddSeconds(1) >= date)
                    {
                        return;
                    }

                    if (character.TokenEnergyBar > 0)
                    {
                        character.UpdateTokenPointsBar(-5).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    if (character.TokenEnergyBar == 0 && character.TokenGauge > 0)
                    {
                        character.TokenGauge = 0;
                        character.Session.SendSpFtptPacket();
                    }

                    character.LastTokenEnergy = date;
                    break;
            }

        }
        
        private void ProcessTokenEnergyBar(DateTime date, IPlayerEntity character)
        {
            switch (character.Morph)
            {
                case (byte)MorphType.StoneBreaker:
                case (byte)MorphType.FogHunter:
                case (byte)MorphType.FireStorm:
                case (byte)MorphType.Thunderer:
                    if (character.LastDefence.AddSeconds(15) >= date || character.LastAttack.AddSeconds(15) >= date || character.LastTokenEnergyRefill.AddSeconds(15) >= date)
                    {
                        return;
                    }

                    if (character.LastTokenEnergy.AddSeconds(1) >= date)
                    {
                        return;
                    }

                    if (character.TokenEnergyBar > 0)
                    {
                        character.UpdateTokenPointsBar(-5).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    if (character.TokenEnergyBar == 0 && character.TokenGauge > 0)
                    {
                        character.TokenGauge = 0;
                        character.Session.SendSpFtptPacket();
                    }

                    character.LastTokenEnergy = date;
                    break;
            }

        }
        
        private void ProcessTeleportSunWolfTooFar(in DateTime date, in IPlayerEntity character)
        {
            if (character.LastSunWolfTeleport.AddSeconds(3) > date)
            {
                return;
            }

            character.LastSunWolfTeleport = date;
            
            if (character.Morph != (byte)MorphType.Sunchaser)
            {
                return;
            }

            IMateEntity sunWolf = character.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

            if (sunWolf == null)
            {
                return;
            }

            if (character.IsInRange(sunWolf.Position.X, sunWolf.Position.Y, 14))
            {
                return;
            }

            sunWolf.TeleportNearCharacter();
            sunWolf.MapInstance.Broadcast(sunWolf.GenerateTeleportPacket(sunWolf.MapX, sunWolf.MapY));
            sunWolf.BroadcastEffectTarget(sunWolf, EffectType.Teleportation);
        }
        
        // private void ProcessAltar(in DateTime date, IPlayerEntity character)
        // {
        //     if (!character.IsAlive() || character.LastHealth.AddSeconds(2) > date || !character.MapInstance.HasMapFlag(MapFlags.ACT_4))
        //     {
        //         return;
        //     }
        //
        //     IMapInstance currentMap = character.Session.CurrentMapInstance;
        //     IEnumerable<IBattleEntity> altars = new[]
        //     {
        //         currentMap.GetBattleEntity(VisualType.Npc, 69696),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69697),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69698),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69699),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69700),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69701),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69702),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69703),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69704),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69705),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69706),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69707),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69708),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69709),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69710),
        //         currentMap.GetBattleEntity(VisualType.Npc, 69711)
        //     }.Where(altar => altar != null && character.Position.GetDistance(altar.Position) <= 5);
        //
        //     foreach (IBattleEntity altar in altars)
        //     {
        //         if (character.Faction == altar.Faction)
        //         {
        //             ProcessHealing(character);
        //         }
        //         else
        //         {
        //             ProcessDamage(character, altar);
        //         }
        //     }
        //
        //     character.Session.RefreshStat();
        // }
        
        // private void ProcessHealing(IPlayerEntity character)
        // {
        //     switch (character.Faction)
        //     {
        //         case FactionType.Angel:
        //             if (!character.HasBuff(BuffVnums.ANGELIC_POWER))
        //             {
        //                 character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.ANGELIC_POWER, character)).ConfigureAwait(false).GetAwaiter().GetResult();
        //             }
        //             break;
        //         case FactionType.Demon:
        //             if (!character.HasBuff(BuffVnums.DEMONIC_POWER))
        //             {
        //                 character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.DEMONIC_POWER, character)).ConfigureAwait(false).GetAwaiter().GetResult();
        //             }
        //             break;
        //     }
        //     
        //     const int healAmount = 2500;
        //     int actualHealAmount = Math.Min(healAmount, character.MaxHp - character.Hp);
        //     character.Hp += actualHealAmount;
        //
        //     character.Session.PlayerEntity.EmitEventAsync(new BattleEntityHealEvent
        //     {
        //         Entity = character.Session.PlayerEntity,
        //         HpHeal = actualHealAmount
        //     });
        // }

        // private void ProcessDamage(IPlayerEntity character, IBattleEntity altar)
        // {
        //     int damageAmount = (int)(character.MaxHp * 0.1);
        //     
        //     if (character.Hp - damageAmount <= 1)
        //     {
        //         damageAmount = character.Hp - 1;
        //     }
        //     
        //     switch (character.Faction)
        //     {
        //         case FactionType.Angel:
        //             if (!character.HasBuff(BuffVnums.ANGELIC_JUDGEMENT))
        //             {
        //                 character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.ANGELIC_JUDGEMENT, character)).ConfigureAwait(false).GetAwaiter().GetResult();
        //             }
        //             break;
        //         case FactionType.Demon:
        //             if (!character.HasBuff(BuffVnums.DEMONIC_JUDGEMENT))
        //             {
        //                 character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.DEMONIC_JUDGEMENT, character)).ConfigureAwait(false).GetAwaiter().GetResult();
        //             }
        //             break;
        //     }
        //     
        //     character.Hp -= damageAmount;
        //     
        //     if (damageAmount == 0)
        //     {
        //         return;
        //     }
        //     
        //     altar.BroadcastDamage(damageAmount);
        // }
        
        private void Update(in DateTime date, in IPlayerEntity character)
        {
            ProcessClockPacket(date);
            //ProcessAltar(date, character);
            ProcessTartHapendamBuffs (character);
            ProcessPrestigeBossTimeWarnings(date, character);
            ProcessTeleportSunWolfTooFar(date, character);
            ProcessEnergyBar(date, character);
            ProcessTokenEnergyBar(date, character);
            ProcessRewards(date, character);
            ProcessRevivalEvents(date, character);
            RemoveManaOnDeath(date, character);
            HealthHeal(date, character);
            ProcessSpecialist(date, character);
            ProcessArchmageTeleport(date, character);
            _bCardTickSystem.ProcessUpdate(character, date);
            _skillCooldownSystem.Update(character, date);
            _snackFoodSystem.ProcessUpdate(character, date);
            ProcessSkillReset(date, character);
            ProcessSpSkillReset(date, character);
            ProcessWeedingBuffCheck(character, date);
            ProcessFamilyLevelBuff(character);
            ProcessMinigameEffect(date, character);
            ProcessRandomTeleport(date, character);
            ProcessMuteMessage(date, character);
            ProcessArenaImmunity(date, character);
            ProcessBlockedZone(character);
            ProcessBuddha(character, date);
            ProcessMeteorites(character, date);

            RefreshRaidStat(date, character);
            ProcessCharacterEffects(date, character);
            ProcessBubble(date, character);
            ProcessItemsToRemove(date, character);
            ProcessExpiredStaticBonus(date, character);

            ProcessMeditation(date, character);
            ProcessComboSkills(date, character);
            ProcessSpyOut(date, character);
            ProcessBattlePassQuestExpired(character, date);
            ProcessFoodBuffAndDecrease(character, date);
            
            if (!character.UseSp || character.Specialist == null)
            {
                return;
            }

            ProcessSpPointsRemoving(date, character);
        }

        
        private void ProcessFoodBuffAndDecrease(in IPlayerEntity player, in DateTime date)
        {
            if (player.LastFoodProcess + ProcessFoodInterval > date)
            {
                return;
            }

            player.LastFoodProcess = date;

            if (player.FoodValue == 0)
            {
                return;
            }

            player.Session.EmitEvent(new DecreaseFoodValueEvent(42));
        }
        
        private void ProcessBattlePassQuestExpired(in IPlayerEntity player, in DateTime date)
        {
            if (player.BattlePassProcess + ProcessBattlePassThings > date)
            {
                return;
            }

            player.BattlePassProcess = date;
            player.Session.EmitEvent(new RemoveAllBattlePassQuestExpiredEvent());
            player.Session.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.StayLoggedXMinute));
        }

        private void ProcessFamilyLevelBuff(IPlayerEntity character)
        {
            var allBuffIds = _familyLevelBuffConfiguration.FamilyLevelBuffs.SelectMany(x => x.BuffVnums).ToHashSet();
            if (allBuffIds.Count == 0)
            {
                return;
            }

            Buff[] activeBuffs = character.BuffComponent.GetAllBuffs(b => allBuffIds.Contains(b.CardId)).ToArray();

            if (!character.IsInFamily())
            {
                if (activeBuffs.Length > 0)
                {
                    character.RemoveBuffAsync(true, activeBuffs).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                return;
            }

            List<int> desiredBuffs = _familyLevelBuffConfiguration.FamilyLevelBuffs.FirstOrDefault(x => x.Level == character.Family.Level)?.BuffVnums ?? [];
            if (desiredBuffs.Count == 0)
            {
                return;
            }

            var desiredSet = desiredBuffs.ToHashSet();
            Buff[] toRemove = activeBuffs.Where(b => !desiredSet.Contains(b.CardId)).ToArray();
            if (toRemove.Length > 0)
            {
                character.RemoveBuffAsync(true, toRemove).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            desiredSet.Where(id => !character.HasBuff((BuffVnums)id))
                .Select(id => _buffFactory.CreateBuff(id, character))
                .Where(buff => buff != null)
                .ToList()
                .ForEach(buff => character.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult());
        }

        private void ProcessMeteorites(in IPlayerEntity character, in DateTime date)
        {
            if (!character.SkillComponent.ArchMageMeteorites.Any())
            {
                return;
            }

            var meteorites = character.SkillComponent.ArchMageMeteorites.Take(3).ToList();
            _asyncEventPipeline.ProcessEventAsync(new MonsterSummonEvent(character.MapInstance, meteorites, character)).ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (ToSummon toSummon in meteorites)
            {
                character.SkillComponent.ArchMageMeteorites.Remove(toSummon);
            }
        }

        private void ProcessBuddha(in IPlayerEntity character, in DateTime date)
        {
            if (!character.SkillComponent.BuddhaWordsActivated)
            {
                return;
            }

            if (character.SkillComponent.LastBuddhaTick > date)
            {
                return;
            }

            character.SkillComponent.LastBuddhaTick = date.AddSeconds(5);

            SkillDTO buddhaSkill = _skillsManager.GetSkill((short)SkillsVnums.BUDDHAS_WORDS);
            if (buddhaSkill == null)
            {
                return;
            }

            if (character.Mp - buddhaSkill.MpCost <= 0)
            {
                character.Session.RemoveBuddha();
                return;
            }

            character.Mp -= buddhaSkill.MpCost;
            character.Session.RefreshStat();

            IReadOnlyList<IBattleEntity> alliesInRange = character.Position.GetAlliesInRange(character, buddhaSkill.AoERange);
            int heal = character.Level * 2;
            bool removeDebuffs = _randomGenerator.RandomNumber() <= 25;

            foreach (IBattleEntity battleEntity in alliesInRange)
            {
                battleEntity.EmitEvent(new BattleEntityHealEvent
                {
                    HpHeal = heal,
                    Entity = battleEntity
                });

                if (removeDebuffs)
                {
                    battleEntity.RemoveNegativeBuffs(4);
                }
            }
        }

        private void ProcessBlockedZone(in IPlayerEntity character)
        {
            if (character.Session.IsGameMaster())
            {
                return;
            }

            if (character.MapInstance == null)
            {
                return;
            }

            if (!character.MapInstance.IsBlockedZone(character.Position.X, character.Position.Y))
            {
                return;
            }

            Position getRandomPosition = character.MapInstance.GetRandomPosition();
            character.ChangePosition(getRandomPosition);
            character.Session.SendCondPacket();
            character.Session.BroadcastTeleportPacket();
        }

        private void ProcessArenaImmunity(in DateTime date, in IPlayerEntity character)
        {
            if (character.MapInstance?.MapInstanceType is not (MapInstanceType.ArenaInstance or MapInstanceType.Alzanor))
            {
                return;
            }

            if (!character.ArenaImmunity.HasValue)
            {
                return;
            }

            if (character.ArenaImmunity.Value.AddSeconds(5) > date)
            {
                return;
            }

            character.ArenaImmunity = null;
            character.Session.SendChatMessage(character.Session.GetLanguage(GameDialogKey.ARENA_CHATMESSAGE_PVP_ACTIVE), ChatMessageColorType.Yellow);
        }

        private void ProcessRandomTeleport(in DateTime date, in IPlayerEntity character)
        {
            if (!character.RandomMapTeleport.HasValue)
            {
                return;
            }

            if (character.RandomMapTeleport.Value.AddSeconds(1.5) > date)
            {
                return;
            }

            Position randomPosition = character.MapInstance.GetRandomPosition();
            character.TeleportOnMap(randomPosition.X, randomPosition.Y);
            character.RandomMapTeleport = null;
            character.BroadcastEffectGround(EffectType.VehicleTeleportation, character.PositionX, character.PositionY, false);
        }

        private void ProcessMuteMessage(in DateTime date, in IPlayerEntity character)
        {
            if (!character.MuteRemainingTime.HasValue)
            {
                return;
            }

            if (character.GameStartDate.AddSeconds(1) > date)
            {
                return;
            }

            character.MuteRemainingTime -= date - character.LastMuteTick;
            character.LastMuteTick = date;
            if (character.MuteRemainingTime.Value.TotalMilliseconds <= 0)
            {
                character.MuteRemainingTime = null;
                character.LastChatMuteMessage = null;

                AccountPenaltyDto penalty = character.Session.Account.Logs.FirstOrDefault(x => x.RemainingTime.HasValue && x.PenaltyType == PenaltyType.Muted);
                if (penalty == null)
                {
                    return;
                }

                penalty.RemainingTime = null;

                return;
            }

            character.LastChatMuteMessage ??= DateTime.MinValue;
            if (character.LastChatMuteMessage.Value.AddMinutes(1) > date)
            {
                return;
            }

            character.LastChatMuteMessage = date;
            string timeLeft = character.MuteRemainingTime.Value.ToString(@"hh\:mm\:ss");
            character.Session.SendChatMessage(character.Session.GetLanguageFormat(GameDialogKey.MUTE_CHATMESSAGE_TIME_LEFT, timeLeft), ChatMessageColorType.Green);
        }

        private void ProcessExpiredStaticBonus(in DateTime date, in IPlayerEntity character)
        {
            if (character.Bonus == null)
            {
                return;
            }

            if (!character.Bonus.Any())
            {
                return;
            }

            if (character.BonusesToRemove.AddMinutes(1) > date)
            {
                return;
            }

            character.BonusesToRemove = date;
            character.Session.EmitEvent(new CharacterBonusExpiredEvent());
        }

        private void ProcessWeedingBuffCheck(in IPlayerEntity character, in DateTime date)
        {
            if (!character.IsInGroup())
            {
                return;
            }

            if (character.CheckWeedingBuff == null)
            {
                return;
            }

            if (character.CheckWeedingBuff.Value.AddSeconds(2) > date)
            {
                return;
            }

            character.CheckWeedingBuff = null;
            character.Session.EmitEvent(new GroupWeedingEvent());
        }

        private void ProcessSpSkillReset(in DateTime date, in IPlayerEntity character)
        {
            if (!character.SkillComponent.ResetSpSkillCooldowns.HasValue)
            {
                return;
            }

            if (character.SkillComponent.ResetSpSkillCooldowns.Value.AddMilliseconds(500) > date)
            {
                return;
            }

            if (!character.UseSp || character.Specialist == null)
            {
                return;
            }

            character.Session.LearnSpSkill(_skillsManager, _gameLanguage);
            character.SkillComponent.ResetSpSkillCooldowns = null;
        }

        private void ProcessSkillReset(in DateTime date, in IPlayerEntity character)
        {
            if (!character.SkillComponent.ResetSkillCooldowns.HasValue)
            {
                return;
            }

            if (character.SkillComponent.ResetSkillCooldowns.Value.AddMilliseconds(500) > date)
            {
                return;
            }

            character.Session.LearnAdventurerSkill(_skillsManager, _gameLanguage);
            character.SkillComponent.ResetSkillCooldowns = null;
        }

        private void ProcessArchmageTeleport(in DateTime date, in IPlayerEntity character)
        {
            if (character.SkillComponent.SendTeleportPacket == null)
            {
                return;
            }

            if (character.SkillComponent.SendTeleportPacket.Value.AddMilliseconds(500) > date)
            {
                return;
            }

            SkillInfo fakeTeleport = character.GetFakeTeleportSkill();
            character.SkillComponent.SendTeleportPacket = null;
            character.Session.SendSkillCooldownResetAfter(fakeTeleport.CastId, (short)character.ApplyCooldownReduction(fakeTeleport));
        }

        private void ProcessSpecialist(in DateTime date, in IPlayerEntity character)
        {
            if (!character.SpCooldownEnd.HasValue)
            {
                return;
            }

            if (character.SpCooldownEnd.Value > date)
            {
                return;
            }

            character.SpCooldownEnd = null;
            character.Session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_CHATMESSAGE_TRANSFORM_DISAPPEAR, character.Session.UserLanguage), ChatMessageColorType.Yellow);
            character.Session.ResetSpCooldownUi();
        }

        private void ProcessItemsToRemove(in DateTime date, in IPlayerEntity character)
        {
            if (character.ItemsToRemove.AddSeconds(10) > date)
            {
                return;
            }

            character.ItemsToRemove = date;
            character.Session.EmitEvent(new InventoryExpiredItemsEvent());
        }

        private void ProcessSpyOut(DateTime time, IPlayerEntity character)
        {
            if (!_spyOutManager.ContainsSpyOut(character.Id))
            {
                return;
            }

            if (character.SpyOutStart.AddMinutes(2) > time)
            {
                return;
            }

            character.Session.SendObArPacket();
            _spyOutManager.RemoveSpyOutSkill(character.Id);
        }
        
        private void ProcessRewards(in DateTime date, in IPlayerEntity character)
        {
            DateTime gameStart = character.GameStartDate;
            TimeSpan timeElapsedSinceGameStart = date - gameStart;

            character.Session.EmitEvent(new DailyRewardEvent
            {
                TimeSinceGameStart = timeElapsedSinceGameStart,
                CharacterSystem = this
            });
        }
        private static void ProcessRevivalEvents(in DateTime date, IPlayerEntity character)
        {
            if (character.RevivalDateTimeForExecution <= date)
            {
                character.DisableRevival();
                character.Session.EmitEvent(new RevivalReviveEvent(character.RevivalType, character.ForcedType));
                return;
            }

            if (character.IsAlive())
            {
                return;
            }

            if (character.AskRevivalDateTimeForExecution > date)
            {
                return;
            }

            character.DisableAskRevival();
            character.Session.EmitEvent(new RevivalAskEvent(character.AskRevivalType));
        }

        private void ProcessBubble(in DateTime date, IPlayerEntity character)
        {
            if (!character.IsUsingBubble())
            {
                return;
            }

            if (character.Bubble.AddMinutes(30) > date)
            {
                return;
            }

            character.RemoveBubble();
        }

        private void RemoveManaOnDeath(in DateTime date, IPlayerEntity character)
        {
            if (character.LastHealth.AddSeconds(1) > date)
            {
                return;
            }

            if (character.Hp != 0)
            {
                return;
            }

            character.Mp = 0;
            character.Session.RefreshStat();
            character.LastHealth = date;
        }

        private void HealthHeal(in DateTime date, IPlayerEntity character)
        {
            if (!character.IsAlive())
            {
                return;
            }

            if (character.LastHealth.AddSeconds(2) > date && (!character.IsSitting || character.LastHealth.AddSeconds(1.5) > date))
            {
                return;
            }

            character.LastHealth = date;
            if (character.LastDefence.AddSeconds(4) > date || character.LastSkillUse.AddSeconds(2) > date)
            {
                return;
            }

            character.Hp += character.Hp + character.HealthHpLoad() < character.MaxHp ? character.HealthHpLoad() : character.MaxHp - character.Hp;
            character.Mp += character.Mp + character.HealthMpLoad() < character.MaxMp ? character.HealthMpLoad() : character.MaxMp - character.Mp;
            character.Session.RefreshStat();
        }

        private void ProcessMinigameEffect(in DateTime date, IPlayerEntity character)
        {
            if (character.LastEffectMinigame.AddSeconds(3) > date)
            {
                return;
            }

            if (character.CurrentMinigame == 0)
            {
                return;
            }

            character.Session.BroadcastEffectInRange(character.CurrentMinigame);
            character.LastEffectMinigame = date;
        }

        private void RefreshRaidStat(in DateTime date, IPlayerEntity character)
        {
            if (!character.IsInRaidParty)
            {
                return;
            }

            if (character.Session.CurrentMapInstance?.MapInstanceType != MapInstanceType.RaidInstance)
            {
                return;
            }

            character.Session.SendRaidPacket(RaidPacketType.REFRESH_MEMBERS_HP_MP);
        }

        private static void ProcessCharacterEffects(in DateTime date, IPlayerEntity character)
        {
            if (character.IsInvisible())
            {
                return;
            }
            
            if (character.MapInstance.IsAct6PvpInstance)
            {
                if ((date - character.LastAct6PvpEffects).TotalSeconds >= 1)
                {
                    character.LastAct6PvpEffects = date;

                    EffectType effectType = character.Faction switch
                    {
                        FactionType.Demon => EffectType.RedTeam,
                        FactionType.Angel => EffectType.BlueTeam,
                        FactionType.Neutral => EffectType.BlueTeam,
                        _ => throw new InvalidOperationException("Invalid faction type.")
                    };

                    character.Session.BroadcastEffect(effectType);
                }
            }

            
            if (character.RainbowBattleComponent.IsInRainbowBattle)
            {
                if ((date - character.LastRainbowEffects).TotalSeconds >= 1)
                {
                    character.LastRainbowEffects = date;
                    RainbowBattleTeamType team = character.RainbowBattleComponent.Team;
                    EffectType effectType = team switch
                    {
                        RainbowBattleTeamType.Red => EffectType.RedTeam,
                        RainbowBattleTeamType.Blue => EffectType.BlueTeam
                    };

                    character.Session.BroadcastEffect(effectType);

                    if (character.RainbowBattleComponent.IsFrozen)
                    {
                        character.Session.BroadcastEffect(EffectType.Frozen);
                    }
                }
            }

            if (character.AlzanorComponent.IsInAlzanorEvent)
            {
                if ((date - character.LastRainbowEffects).TotalSeconds >= 1)
                {
                    character.LastRainbowEffects = date;
                    AlzanorTeamType team = character.AlzanorComponent.Team;
                    EffectType effectType = team switch
                    {
                        AlzanorTeamType.Red => EffectType.RedTeam,
                        AlzanorTeamType.Blue => EffectType.BlueTeam
                    };

                    character.Session.BroadcastEffect(effectType);
                }
            }

            if (character.IsFrozenByGlacerus())
            {
                character.Session.BroadcastEffect(EffectType.Frozen);
            }

            if (character.LastEffect.AddSeconds(5) > date)
            {
                return;
            }

            character.LastEffect = date;

            if (character.HasBuff(BuffVnums.WEDDING))
            {
                character.Session.BroadcastEffect(EffectType.MediumHearth);
            }

            if (character.IsInRaidParty)
            {
                if (character.IsRaidLeader(character.Id))
                {
                    character.Session.BroadcastEffect(EffectType.OtherRaidLeader, new ExceptRaidBroadcast(character.Raid.Id));
                    character.Session.BroadcastEffect(EffectType.OwnRaidLeader, new InRaidBroadcast(character.Raid));
                }
                else
                {
                    if (character.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }

                    character.Session.BroadcastEffect(EffectType.OtherRaidMember, new ExceptRaidBroadcast(character.Raid.Id));
                    character.Session.BroadcastEffect(EffectType.OwnRaidMember, new InRaidBroadcast(character.Raid));
                }
            }

            if (character.Specialist is { ItemVNum: (short)ItemVnums.JAJAMARU_SP } && character.UseSp)
            {
                if (character.MateComponent.GetTeamMember(x => x.MateType == MateType.Partner && x.MonsterVNum == (short)MonsterVnum.SAKURA) != null)
                {
                    character.BroadcastEffectInRange(EffectType.Heart);
                }
            }

            GameItemInstance amulet = character.Amulet;
            if (amulet == null)
            {
                return;
            }

            if (character.Invisible || character.CheatComponent.IsInvisible)
            {
                return;
            }

            if (amulet.GameItem.EffectValue == -1)
            {
                return;
            }

            if (amulet.ItemVNum is (int)ItemVnums.DRACO_AMULET or (int)ItemVnums.GLACERUS_AMULET)
            {
                character.Session.BroadcastEffectInRange(amulet.GameItem.EffectValue + (character.Class == ClassType.Adventurer ? 0 : (byte)character.Class - 1));
            }
            else
            {
                character.Session.BroadcastEffectInRange(amulet.GameItem.EffectValue);
            }
        }

        private void ProcessMeditation(in DateTime date, IPlayerEntity character)
        {
            if (!_meditationManager.HasMeditation(character))
            {
                return;
            }

            IReadOnlyList<Buff> buffs = character.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad);
            character.RemoveBuffAsync(false, buffs.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();

            // Get all the meditations from the character
            List<(short, DateTime)> meditations = _meditationManager.GetAllMeditations(character);
            foreach ((short meditationId, DateTime meditationStart) in meditations.ToList())
            {
                // If that meditation is not ready to start, go next
                if (meditationStart >= date)
                {
                    continue;
                }

                character.Session.BroadcastEffectInRange(meditationId == (short)BuffVnums.SPIRIT_OF_STRENGTH
                    ? EffectType.MeditationFinalStage
                    : EffectType.MeditationFirstStage);

                // Removes one buff or another depending of the current meditation state
                Buff firstBuff, secondBuff;
                switch (meditationId)
                {
                    case (short)BuffVnums.SPIRIT_OF_ENLIGHTENMENT:
                        firstBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_TEMPERANCE);
                        secondBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_STRENGTH);
                        character.Session.SendSound(SoundType.MEDITATION_FIRST);
                        break;
                    case (short)BuffVnums.SPIRIT_OF_TEMPERANCE:
                        firstBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_ENLIGHTENMENT);
                        secondBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_STRENGTH);
                        character.Session.SendSound(SoundType.MEDITATION_SECOND);
                        break;
                    case (short)BuffVnums.SPIRIT_OF_STRENGTH:
                        firstBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_ENLIGHTENMENT);
                        secondBuff = character.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_TEMPERANCE);
                        character.Session.SendSound(SoundType.MEDITATION_THIRD);
                        break;
                    default:
                        firstBuff = null;
                        secondBuff = null;
                        break;
                }

                character.RemoveBuffAsync(false, firstBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                character.RemoveBuffAsync(false, secondBuff).ConfigureAwait(false).GetAwaiter().GetResult();

                Buff actualBuff = _buffFactory.CreateBuff(meditationId, character);
                character.AddBuffAsync(actualBuff).GetAwaiter().GetResult();
                _meditationManager.RemoveMeditation(character, meditationId);
            }
        }

        private void ProcessComboSkills(in DateTime date, IPlayerEntity character)
        {
            ComboSkillState comboSkillState = character.GetComboState();
            if (comboSkillState == null)
            {
                return;
            }

            if (character.AngelElement.HasValue)
            {
                return;
            }

            if (!character.LastSkillCombo.HasValue)
            {
                return;
            }

            if (character.LastSkillCombo.Value.AddSeconds(5) > date)
            {
                return;
            }

            character.LastSkillCombo = null;
            character.Session.SendMsCPacket(0);
            character.Session.RefreshQuicklist();
            character.CleanComboState();
        }

        private void ProcessSpPointsRemoving(in DateTime date, IPlayerEntity character)
        {
            if (character.HasBuff(BuffVnums.RAINBOW_ENERGY))
            {
                return;
            }

            if (!character.IsAlive())
            {
                return;
            }

            if (character.Skills.All(s => s.LastUse <= DateTime.UtcNow))
            {
                return;
            }

            if (character.LastSkillUse.AddSeconds(15) <= date)
            {
                if (!character.IsRemovingSpecialistPoints)
                {
                    return;
                }

                character.IsRemovingSpecialistPoints = false;
                character.Session.SendScpPacket(0);
                character.Session.RefreshSpPoint();
                character.InitialScpPacketSent = false;
                return;
            }

            if (character.LastSpRemovingProcess.AddSeconds(1) <= date)
            {
                character.LastSpRemovingProcess = date;
                character.IsRemovingSpecialistPoints = true;
                RemoveSpecialistPoints(character);
            }

            if (character.LastSpPacketSent + ProcessSpInterval > date)
            {
                return;
            }

            character.Session.RefreshSpPoint();
            character.LastSpPacketSent = date;
        }

        private void RemoveSpecialistPoints(IPlayerEntity character)
        {
            if (!character.InitialScpPacketSent)
            {
                character.Session.SendScpPacket(1);
                character.InitialScpPacketSent = true;
            }

            byte spPoints = character.Specialist.GameItem.SpPointsUsage;

            if (character.SpPointsBasic == 0 && character.SpPointsBonus == 0)
            {
                if (character.IsOnVehicle)
                {
                    character.Session.EmitEvent(new RemoveVehicleEvent(true));
                }

                character.Session.EmitEvent(new SpUntransformEvent());
                character.Session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_NO_POINTS, character.Session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            character.SpPointsBasic = character.SpPointsBasic - spPoints < 0 ? 0 : character.SpPointsBasic - spPoints;
        }
        
        private void AddPrivateCharacter(IPlayerEntity character)
        {
            if (_characters.Contains(character))
            {
                return;
            }

            _charactersById.TryAdd(character.Id, character);
            _characters.Add(character);
        }

        private void RemovePrivateCharacter(IPlayerEntity character)
        {
            _charactersById.TryRemove(character.Id, out _);
            _characters.Remove(character);
        }
    }
}