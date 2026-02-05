// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.CoreImpl.Maps.Systems;
using Plugin.CoreImpl.Pathfinding;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.Raids;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace Plugin.CoreImpl.Maps
{
    public class MapInstance : SessionsContainer, IMapInstance
    {
        private static readonly TimeSpan _timeToIdleState = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("WINGSEMU_TICK_IDLE_DELAY") ?? "10"));

        private readonly BattleSystem _battleSystem;
        private readonly CharacterSystem _characterSystem;
        private readonly DropSystem _dropSystem;
        private readonly IEntityIdManager _entityIdManager;

        private readonly HashSet<MapFlags> _flags = new();
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly MateSystem _mateSystem;
        private readonly MonsterSystem _monsterSystem;
        private readonly NpcSystem _npcSystem;
        private readonly IPortalFactory _portalFactory;

        private readonly IRandomGenerator _randomGenerator;
        private readonly IMapSystem[] _systems;
        private readonly IFishManager _fishManager;
        private readonly ITickManager _tickManager;
        private readonly ILandOfDeathManager _landOfDeathManager;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly FamilyLevelBuffConfiguration _familyLevelBuffConfiguration;
        private readonly PrestigeConfiguration _prestigeConfiguration;
        private List<Position> _cells;

        private DateTime? _idleRequest;
        private bool _isTickRefreshRequested;

        public MapInstance(Map map, MapInstanceType type, ITickManager tickManager, GameMinMaxConfiguration minMaxConfiguration,
            ISpyOutManager spyOutManager, ISkillsManager skillsManager, IGameLanguageService languageService, IMeditationManager meditationManager, IAsyncEventPipeline asyncEventPipeline,
            IRandomGenerator randomGenerator, IBCardEffectHandlerContainer bCardEffectHandlerContainer, IBuffFactory buffFactory,
            IPortalFactory portalFactory, IGameItemInstanceFactory gameItemInstanceFactory, ISpPartnerConfiguration spPartnerConfiguration, IMonsterTalkingConfig monsterTalkingConfig,
            SerializableGameServer serializableGameServer, IFishManager fishManager, ITrainerSpecialistConfiguration trainerSpecialistConfiguration, 
            PetMaxLevelConfiguration petMaxLevelConfiguration, IEvtbConfiguration evtbConfiguration, ILandOfDeathManager landOfDeathManager, RainbowBattleConfiguration rainbowBattleConfiguration, FamilyLevelBuffConfiguration familyLevelBuffConfiguration, PrestigeConfiguration prestigeConfiguration)
        {
            Id = Guid.NewGuid();

            Grid = map.Grid;
            Width = map.Width;
            Height = map.Height;
            MapId = map.MapId;
            MapVnum = map.MapVnum;
            MapNameId = map.MapNameId;
            Music = map.Music;
            IsBossRoom = map.IsBossRoom;

            foreach (MapFlags flag in map.Flags)
            {
                _flags.Add(flag);
            }

            IsPvp = HasMapFlag(MapFlags.HAS_PVP_ENABLED) || HasMapFlag(MapFlags.HAS_PVP_FACTION_ENABLED) || HasMapFlag(MapFlags.HAS_PVP_FAMILY_ENABLED);
            ShopAllowed = !HasMapFlag(MapFlags.HAS_USER_SHOPS_DISABLED) && !HasMapFlag(MapFlags.ACT_4) && !IsPvp && type == MapInstanceType.BaseMapInstance;

            MapInstanceType = type;

            IPathFinder pathfinder = new PathFinder(Grid, Width, Height);

            _tickManager = tickManager;
            _randomGenerator = randomGenerator;
            _portalFactory = portalFactory;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _npcSystem = new NpcSystem(bCardEffectHandlerContainer, this, asyncEventPipeline, randomGenerator, buffFactory, languageService, pathfinder, monsterTalkingConfig);
            _monsterSystem = new MonsterSystem(randomGenerator, bCardEffectHandlerContainer, asyncEventPipeline, this, buffFactory, languageService, pathfinder, monsterTalkingConfig, 
                fishManager, trainerSpecialistConfiguration, petMaxLevelConfiguration, rainbowBattleConfiguration);
            _characterSystem = new CharacterSystem(bCardEffectHandlerContainer, buffFactory, meditationManager, asyncEventPipeline, this, spyOutManager, randomGenerator, skillsManager,
                minMaxConfiguration, languageService, serializableGameServer, gameItemInstanceFactory, evtbConfiguration, landOfDeathManager, familyLevelBuffConfiguration, prestigeConfiguration);
            _mateSystem = new MateSystem(bCardEffectHandlerContainer, languageService, minMaxConfiguration, this, randomGenerator, buffFactory, languageService, spPartnerConfiguration);
            _battleSystem = new BattleSystem(asyncEventPipeline, this, bCardEffectHandlerContainer, languageService, buffFactory, randomGenerator);
            _dropSystem = new DropSystem(this, randomGenerator);
            _entityIdManager = new GenericEntityIdManager();

            _systems = new IMapSystem[]
            {
                _monsterSystem,
                _characterSystem,
                _npcSystem,
                _mateSystem,
                _dropSystem,
                _battleSystem
            };

            SetIdle();
        }


        public MapInstanceState State { get; private set; }

        public bool IsDance { get; set; }

        public short? MapMusic { get; set; }

        public bool IsPvp { get; set; }

        public byte MapIndexX { get; set; }

        public byte MapIndexY { get; set; }

        public Guid Id { get; }
        public MapInstanceType MapInstanceType { get; }

        public IReadOnlyList<byte> Grid { get; }
        public int Width { get; }
        public int Height { get; }
        public int MapId { get; }
        public int Music { get; }
        public int MapVnum { get; }
        public int MapNameId { get; }
        
        public bool IsBossRoom { get; }
        public bool ShopAllowed { get; }
        public bool AIDisabled { get; set; }
        public bool IsAct6PvpInstance { get; set; }
        public List<ITimeSpacePortalEntity> TimeSpacePortals { get; } = new();
        public List<MapDesignObject> MapDesignObjects { get; set; } = new();
        public List<IPortalEntity> Portals { get; } = new();

        public override void RegisterSession(IClientSession session)
        {
            base.RegisterSession(session);
            AddCharacter(session.PlayerEntity);
            foreach (IMateEntity activeMate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                AddMate(activeMate);
            }

            ActivateMap();
        }

        public override void UnregisterSession(IClientSession session)
        {
            base.UnregisterSession(session);
            RemoveCharacter(session.PlayerEntity);
            foreach (IMateEntity activeMate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                RemoveMate(activeMate);
            }

            if (State == MapInstanceState.Running && Sessions.Count == 0)
            {
                RequestIdleState();
            }
        }

        public IReadOnlyList<string> GetEntitiesOnMapPackets(bool onlyItemsAndPortals = false)
        {
            var packetCache = new List<string>();

            foreach (IPortalEntity portal in Portals)
            {
                packetCache.Add(portal.GenerateGp());
            }

            if (!onlyItemsAndPortals)
            {
                bool firstBossSent = false;
                foreach (IMonsterEntity monster in GetAliveMonsters())
                {
                    if (!monster.IsStillAlive)
                    {
                        continue;
                    }

                    packetCache.Add(monster.GenerateIn());
                    if (monster.IsBoss && monster.IsTarget)
                    {
                        packetCache.Add(monster.GenerateRaidBossPacket(firstBossSent));
                        firstBossSent = true;
                    }

                    TryAddScal(packetCache, monster);
                    packetCache.AddRange(monster.GenerateConstBuffEffects());
                }
            }

            foreach (MapItem mapItem in Drops)
            {
                packetCache.Add(mapItem.GenerateIn());
            }

            if (!onlyItemsAndPortals)
            {
                foreach (INpcEntity npc in GetAliveNpcs())
                {
                    if (!npc.IsStillAlive)
                    {
                        continue;
                    }

                    packetCache.Add(npc.GenerateIn());
                    if (npc.ShopNpc != null)
                    {
                        packetCache.Add($"shop 2 {npc.Id} 1 {(byte)npc.ShopNpc.MenuType} {npc.ShopNpc.ShopType} {npc.ShopNpc.Name}");
                    }

                    TryAddScal(packetCache, npc);
                    packetCache.AddRange(npc.GenerateConstBuffEffects());
                }

                foreach (INpcEntity npc in GetPassiveNpcs())
                {
                    if (!npc.IsStillAlive)
                    {
                        continue;
                    }

                    packetCache.Add(npc.GenerateIn());
                    if (npc.ShopNpc != null)
                    {
                        packetCache.Add($"shop 2 {npc.Id} 1 {(byte)npc.ShopNpc.MenuType} {npc.ShopNpc.ShopType} {npc.ShopNpc.Name}");
                    }

                    if (npc.TimeSpaceInfo != null)
                    {
                        packetCache.Add(npc.GenerateEffectGround(EffectType.BlueTimeSpace, npc.PositionX, npc.PositionY, false));
                    }

                    if (npc.RainbowFlag != null)
                    {
                        packetCache.Add(npc.GenerateFlagPacket());
                    }

                    TryAddScal(packetCache, npc);
                    packetCache.AddRange(npc.GenerateConstBuffEffects());
                }
            }

            return packetCache;
        }

        public IBattleEntity GetBattleEntity(VisualType type, long id)
        {
            switch (type)
            {
                case VisualType.Monster:
                    return GetMonsterById(id);
                case VisualType.Player:
                    return GetCharacterById(id);
                case VisualType.Npc:
                    if (id >= 1_000_000)
                    {
                        return GetMateById(id);
                    }

                    return GetNpcById(id);
                default:
                    return null;
            }
        }

        public bool HasMapFlag(MapFlags flags) => _flags.Contains(flags);

        public void LoadPortals(IEnumerable<PortalDTO> tmp = null)
        {
            if (tmp == null)
            {
                return;
            }

            foreach (PortalDTO portalInfo in tmp)
            {
                var sourcePos = new Position(portalInfo.SourceX, portalInfo.SourceY);
                var destPos = new Position(portalInfo.DestinationX, portalInfo.DestinationY);
                IPortalEntity portal = _portalFactory.CreatePortal((PortalType)portalInfo.Type, this, sourcePos, portalInfo.DestinationMapId, destPos, portalInfo.RaidType, portalInfo.MapNameId, portalInfo.LevelRequired, portalInfo.HeroLevelRequired);
                Portals.Add(portal);
            }
        }

        public void Initialize(DateTime date)
        {
            State = MapInstanceState.Running;
            ProcessTick(date);
            State = MapInstanceState.Idle;
        }

        public void Destroy()
        {
            SetIdle();

            foreach (IClientSession session in Sessions.ToList())
            {
                session.ChangeToLastBaseMap();
            }

            foreach (IMapSystem t in _systems)
            {
                t.Clear();
            }
        }

        public Position GetRandomPosition()
        {
            if (_cells is { Count: > 0 })
            {
                return _cells[_randomGenerator.RandomNumber(_cells.Count - 1)];
            }

            _cells = new List<Position>();
            for (short y = 0; y < Height; y++)
            {
                for (short x = 0; x < Width; x++)
                {
                    if (this.CanWalkAround(x, y))
                    {
                        _cells.Add(new Position(x, y));
                    }
                }
            }

            return _cells[_randomGenerator.RandomNumber(_cells.Count - 1)];
        }

        public MapItem PutItem(ushort amount, ref GameItemInstance inv, IClientSession session)
        {
            var possibilities = new List<Position>();

            for (short x = -1; x < 2; x++)
            {
                for (short y = -1; y < 2; y++)
                {
                    possibilities.Add(new Position(x, y));
                }
            }

            short mapX = 0;
            short mapY = 0;
            bool niceSpot = false;
            foreach (Position possibility in possibilities.OrderBy(s => _randomGenerator.RandomNumber()))
            {
                mapX = (short)(session.PlayerEntity.PositionX + possibility.X);
                mapY = (short)(session.PlayerEntity.PositionY + possibility.Y);
                if (this.IsBlockedZone(mapX, mapY))
                {
                    continue;
                }

                niceSpot = true;
                break;
            }

            if (!niceSpot)
            {
                return null;
            }

            if (amount <= 0 || amount > inv.Amount)
            {
                return null;
            }

            GameItemInstance newItemInstance = _gameItemInstanceFactory.DuplicateItem(inv);
            newItemInstance.Amount = amount;
            MapItem droppedItem = new CharacterMapItem(mapX, mapY, newItemInstance, this);
            AddDrop(droppedItem);
            return droppedItem;
        }
        
        public void DespawnAndRemoveMonster(IMonsterEntity monsterEntity)
        {
            DespawnMonster(monsterEntity);
            RemoveMonster(monsterEntity);
        }

        public void DespawnMonster(IMonsterEntity monsterEntity)
        {
            monsterEntity.GenerateDeath();
            Broadcast(monsterEntity.GenerateOut());
        }

        public IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position pos, short distance, Func<IPlayerEntity, bool> predicate) => _characterSystem.GetCharactersInRange(pos, distance, predicate);

        public IReadOnlyList<IPlayerEntity> GetCharactersInRange(Position position, short distance) => GetCharactersInRange(position, distance, null);

        public IReadOnlyList<IPlayerEntity> GetClosestCharactersInRange(Position pos, short distance) => _characterSystem.GetClosestCharactersInRange(pos, distance);

        public IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position pos, short distance, Func<IPlayerEntity, bool> predicate) =>
            _characterSystem.GetAliveCharactersInRange(pos, distance, predicate);

        public IReadOnlyList<IPlayerEntity> GetAliveCharactersInRange(Position position, short distance) => GetAliveCharactersInRange(position, distance, null);

        public IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntities(Func<IBattleEntity, bool> predicate)
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharacters(predicate));
            battleEntitiesInRange.AddRange(GetAliveMates(predicate));
            battleEntitiesInRange.AddRange(GetAliveNpcs(predicate));

            return battleEntitiesInRange;
        }

        public IReadOnlyList<IBattleEntity> GetBattleEntities(Func<IBattleEntity, bool> predicate)
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharacters(predicate));
            battleEntitiesInRange.AddRange(GetAliveMates(predicate));
            battleEntitiesInRange.AddRange(GetAliveNpcs(predicate));
            battleEntitiesInRange.AddRange(GetAliveMonsters(predicate));

            return battleEntitiesInRange;
        }

        public IReadOnlyList<IBattleEntity> GetBattleEntitiesInRange(Position pos, short distance)
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharactersInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetAliveMatesInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetAliveNpcsInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetAliveMonstersInRange(pos, distance));

            return battleEntitiesInRange;
        }

        public IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntitiesInRange(Position pos, short distance) => GetNonMonsterBattleEntitiesInRange(pos, distance, null);

        public IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntitiesInRange(Position pos, short distance, Func<IBattleEntity, bool> predicate)
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharactersInRange(pos, distance, predicate));
            battleEntitiesInRange.AddRange(GetAliveMatesInRange(pos, distance, predicate));
            battleEntitiesInRange.AddRange(GetAliveNpcsInRange(pos, distance, predicate));

            return battleEntitiesInRange;
        }


        public IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntities()
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharacters());
            battleEntitiesInRange.AddRange(GetAliveMates());
            battleEntitiesInRange.AddRange(GetAliveNpcs());

            return battleEntitiesInRange;
        }
        
        public IReadOnlyList<IBattleEntity> GetPlayerBattleEntities()
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetAliveCharacters());

            return battleEntitiesInRange;
        }

        public IReadOnlyList<IMonsterEntity> GetDeadMonsters() => _monsterSystem.GetDeadMonsters();

        public IReadOnlyList<IMonsterEntity> GetAliveMonsters(Func<IMonsterEntity, bool> predicate) => _monsterSystem.GetAliveMonsters(predicate);
        public IReadOnlyList<IMonsterEntity> GetAliveMonstersInRange(Position pos, short distance) => _monsterSystem.GetAliveMonstersInRange(pos, distance);
        public IReadOnlyList<IMonsterEntity> GetClosestMonstersInRange(Position pos, short distance) => _monsterSystem.GetClosestMonstersInRange(pos, distance);

        public IReadOnlyList<IBattleEntity> GetClosestBattleEntitiesInRange(Position pos, short distance)
        {
            var battleEntitiesInRange = new List<IBattleEntity>();

            battleEntitiesInRange.AddRange(GetClosestCharactersInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetClosestMatesInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetClosestNpcsInRange(pos, distance));
            battleEntitiesInRange.AddRange(GetClosestMonstersInRange(pos, distance));

            return battleEntitiesInRange;
        }

        public string Name => $"{nameof(MapInstance)}: {MapInstanceType.ToString()} - {MapId.ToString()}";

        public void ProcessTick(DateTime date)
        {
            try
            {
                if (State == MapInstanceState.Idle)
                {
                    return;
                }
                
                var currentSystems = _systems.ToList();
                foreach (IMapSystem system in currentSystems)
                {
                    system.ProcessTick(date, _isTickRefreshRequested);
                }

                FlushPackets();

                if (_isTickRefreshRequested)
                {
                    _isTickRefreshRequested = false;
                }

                // idle state
                if (Sessions.Count > 0)
                {
                    //Log.Warn($"[TICK_MAP_INSTANCE] {Map.MapId} SessionCount > 0");
                    return;
                }

                _idleRequest ??= date;
                if (_idleRequest.HasValue && _idleRequest.Value + _timeToIdleState < date)
                {
                    SetIdle();
                }
            }
            catch (Exception e)
            {
                Log.Error($"TICK - MapId: {MapId.ToString()}", e);
            }
        }

        public IMonsterEntity GetMonsterById(long id) => _monsterSystem.GetMonsterById(id);
        public IMonsterEntity GetMonsterByUniqueId(Guid id) => _monsterSystem.GetMonsterByUniqueId(id);

        public List<IMonsterEntity> GetMonstersAlive() => _monsterSystem.GetMonstersAlive();
        public List<IMonsterEntity> GetRemainingTotems() => _monsterSystem.GetRemainingTotems();
        public IReadOnlyList<IMonsterEntity> GetAliveMonsters() => _monsterSystem.GetAliveMonsters();

        public void AddMonster(IMonsterEntity entity)
        {
            _monsterSystem.AddMonster(entity);
        }

        public void RemoveMonster(IMonsterEntity entity)
        {
            _monsterSystem.RemoveMonster(entity);
        }

        public void AddEntityToTargets(IMonsterEntity monsterEntity, IBattleEntity target)
        {
            _monsterSystem.AddEntityToTargets(monsterEntity, target);
        }

        public void MonsterRefreshTarget(IMonsterEntity target, IBattleEntity caster, in DateTime time, bool isByAttacking = false)
        {
            _monsterSystem.MonsterRefreshTarget(target, caster, time, isByAttacking);
        }

        public void ForgetAll(IMonsterEntity monsterEntity, in DateTime time, bool clearDamagers = true)
        {
            _monsterSystem.ForgetAll(monsterEntity, time, clearDamagers);
        }

        public void RemoveTarget(IMonsterEntity monsterEntity, IBattleEntity entity, bool checkIfPlayer = false)
        {
            _monsterSystem.RemoveTarget(monsterEntity, entity, checkIfPlayer);
        }

        public void ActivateMode(IMonsterEntity monsterEntity)
        {
            _monsterSystem.ActivateMode(monsterEntity);
        }

        public void DeactivateMode(IMonsterEntity monsterEntity)
        {
            _monsterSystem.DeactivateMode(monsterEntity);
        }

        public void IncreaseMonsterDeathsOnMap()
        {
            _monsterSystem.IncreaseMonsterDeathsOnMap();
        }

        public long MonsterDeathsOnMap() => _monsterSystem.MonsterDeathsOnMap();
        public byte CurrentVessels() => _monsterSystem.CurrentVessels();
        public bool IsSummonLimitReached(int? summonerId, SummonType? summonSummonType) => _monsterSystem.IsSummonLimitReached(summonerId, summonSummonType);

        public IReadOnlyList<INpcEntity> GetAliveNpcs() => _npcSystem.GetAliveNpcs();
        public IReadOnlyList<INpcEntity> GetAliveNpcs(Func<INpcEntity, bool> predicate) => _npcSystem.GetAliveNpcs(predicate);

        public IReadOnlyList<INpcEntity> GetPassiveNpcs() => _npcSystem.GetPassiveNpcs();

        public IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance) => _npcSystem.GetAliveNpcsInRange(pos, distance);
        public IReadOnlyList<INpcEntity> GetClosestNpcsInRange(Position pos, short distance) => _npcSystem.GetClosestNpcsInRange(pos, distance);

        public void NpcRefreshTarget(INpcEntity npcEntity, IBattleEntity target)
        {
            _npcSystem.NpcRefreshTarget(npcEntity, target);
        }

        public IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance, Func<INpcEntity, bool> predicate) => _npcSystem.GetAliveNpcsInRange(pos, distance, predicate);


        public INpcEntity GetNpcById(long id) => _npcSystem.GetNpcById(id);

        public void AddNpc(INpcEntity entity)
        {
            _npcSystem.AddNpc(entity);
        }

        public void RemoveNpc(INpcEntity entity)
        {
            _npcSystem.RemoveNpc(entity);
        }

        public IPlayerEntity GetCharacterById(long id) => _characterSystem.GetCharacterById(id);

        public IReadOnlyList<IPlayerEntity> GetCharacters() => _characterSystem.GetCharacters();

        public IReadOnlyList<IPlayerEntity> GetCharacters(Func<IPlayerEntity, bool> predicate) => _characterSystem.GetCharacters(predicate);

        public IReadOnlyList<IPlayerEntity> GetAliveCharacters() => _characterSystem.GetAliveCharacters();

        public IReadOnlyList<IPlayerEntity> GetAliveCharacters(Func<IPlayerEntity, bool> predicate) => _characterSystem.GetAliveCharacters(predicate);

        public void AddCharacter(IPlayerEntity character)
        {
            _characterSystem.AddCharacter(character);
        }

        public void RemoveCharacter(IPlayerEntity entity)
        {
            _characterSystem.RemoveCharacter(entity);
        }

        public IReadOnlyList<IMateEntity> GetAliveMates() => _mateSystem.GetAliveMates();
        public IReadOnlyList<IMateEntity> GetAliveMates(Func<IMateEntity, bool> predicate) => _mateSystem.GetAliveMates(predicate);
        public IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range) => _mateSystem.GetAliveMatesInRange(position, range);
        public IReadOnlyList<IMateEntity> GetClosestMatesInRange(Position position, short range) => _mateSystem.GetClosestMatesInRange(position, range);

        public IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range, Func<IBattleEntity, bool> predicate) => _mateSystem.GetAliveMatesInRange(position, range, predicate);
        public IMateEntity GetMateById(long mateId) => _mateSystem.GetMateById(mateId);

        public void AddMate(IMateEntity entity)
        {
            _mateSystem.AddMate(entity);
        }

        public void RemoveMate(IMateEntity entity)
        {
            _mateSystem.RemoveMate(entity);
        }

        public IReadOnlyList<MapItem> Drops => _dropSystem.Drops;

        public void AddDrop(MapItem item)
        {
            _dropSystem.AddDrop(item);
        }

        public bool RemoveDrop(long dropId) => _dropSystem.RemoveDrop(dropId);

        public bool HasDrop(long dropId) => _dropSystem.HasDrop(dropId);

        public MapItem GetDrop(long dropId) => _dropSystem.GetDrop(dropId);

        public void AddCastHitRequest(HitProcessable hitProcessable)
        {
            _battleSystem.AddCastHitRequest(hitProcessable);
        }

        public void AddCastBuffRequest(BuffProcessable buffProcessable)
        {
            _battleSystem.AddCastBuffRequest(buffProcessable);
        }

        public void AddHitRequest(HitRequest hitRequest)
        {
            _battleSystem.AddHitRequest(hitRequest);
        }

        public void AddBuffRequest(BuffRequest buffRequest)
        {
            _battleSystem.AddBuffRequest(buffRequest);
        }

        public int GenerateEntityId() => _entityIdManager.GenerateEntityId();

        private static void TryAddScal(ICollection<string> list, IBattleEntity battleEntity)
        {
            if (!battleEntity.ShouldSendScal())
            {
                return;
            }

            list.Add(battleEntity.GenerateScal());
        }

        public void SetInactive()
        {
            SetIdle();
        }

        private void RequestIdleState()
        {
            _idleRequest = DateTime.UtcNow;
            // Log.Warn($"[TICK_IDLE] Requested by: {Map.MapId.ToString()}");
        }

        private void ActivateMap()
        {
            if (State == MapInstanceState.Running)
            {
                _idleRequest = null;
                return;
            }

            // Log.Warn($"[TICK_ACTIVATE] Activation of map: {Map.MapId.ToString()}");
            State = MapInstanceState.Running;
            _isTickRefreshRequested = true;
            _tickManager.AddProcessable(this);
        }

        private void SetIdle()
        {
            if (MapInstanceType == MapInstanceType.Miniland && GetAliveMonsters().Any(s => s.IsSparringMonster))
            {
                return;
            }
            
            if (State == MapInstanceState.Idle)
            {
                return;
            }

            // Log.Warn($"[TICK_IDLE] {Map.MapId.ToString()} is now in idle mode");
            State = MapInstanceState.Idle;
            _idleRequest = null;
            _tickManager.RemoveProcessable(this);
            foreach (IMapSystem t in _systems)
            {
                t.PutIdleState();
            }
        }
    }
}