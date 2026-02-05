using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.InterChannel.Event;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Data.Prestige;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Game.Extensions.Quests;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.CharacterSizeModifiers;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Items.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Minigames;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class CharacterLoadEventHandler : IAsyncEventProcessor<CharacterLoadEvent>
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IAccountService _accountService;
    private readonly IBCardEffectHandlerContainer _bcardHandlerContainer;
    private readonly IBuffFactory _buffFactory;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly ICharacterService _characterService;
    private readonly FamilyConfiguration _familyConfiguration;
    private readonly IFamilyManager _familyManager;
    private readonly SerializableGameServer _gameServer;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _language;
    private readonly IExpirableLockService _expirableLockService;
    private readonly IMapManager _mapManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IMessagePublisher<PlayerConnectedOnChannelMessage> _messagePublisher;
    private readonly IMinilandManager _minilandManager;
    private readonly IPlayerEntityFactory _playerEntityFactory;
    private readonly IQuestFactory _questFactory;
    private readonly IQuestManager _questManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IRevivalManager _revivalManager;
    private readonly IServerApiService _serverApiService;


    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;
    private readonly ISessionService _sessionService;
    private readonly IFamilyService _familyService;
    private readonly ISubActConfiguration _subActConfiguration;
    private readonly ITimeSpaceConfiguration _timeSpaceConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly BattlePassConfiguration _battlePassConfiguration;
    private readonly TrainerSpecialistPetBookConfiguration _trainerSpecialistPetBookConfiguration;
    private readonly INpcMonsterManager _npcMonsterManager;
    public CharacterLoadEventHandler(IServerManager serverManager, IGameLanguageService language, SerializableGameServer gameServer, IBuffFactory buffFactory, ISessionManager sessionManager,
        ICharacterAlgorithm characterAlgorithm, IMinilandManager minilandManager, IMessagePublisher<PlayerConnectedOnChannelMessage> messagePublisher, IRevivalManager revivalManager,
        IQuestManager questManager, FamilyConfiguration familyConfiguration, IServerApiService serverApiService, IRandomGenerator randomGenerator, IMapManager mapManager,
        IReputationConfiguration reputationConfiguration, IItemsManager itemsManager, ISubActConfiguration subActConfiguration, IRankingManager rankingManager,
        IExpirableLockService expirableLockService, ITimeSpaceConfiguration timeSpaceConfiguration, ISessionService sessionService, IGameItemInstanceFactory itemInstanceFactory,
        ICharacterService characterService, IPlayerEntityFactory playerEntityFactory, IMateEntityFactory mateEntityFactory, IFamilyManager familyManager,
        IBCardEffectHandlerContainer bcardHandlerContainer, IQuestFactory questFactory, IAccountService accountService, IEvtbConfiguration evtbConfiguration, 
        IFamilyService familyService, BattlePassConfiguration battlePassConfiguration, TrainerSpecialistPetBookConfiguration trainerSpecialistPetBookConfiguration, INpcMonsterManager npcMonsterManager)
    {
        _serverManager = serverManager;
        _language = language;
        _gameServer = gameServer;
        _buffFactory = buffFactory;
        _sessionManager = sessionManager;
        _characterAlgorithm = characterAlgorithm;
        _minilandManager = minilandManager;
        _messagePublisher = messagePublisher;
        _revivalManager = revivalManager;
        _questManager = questManager;
        _familyConfiguration = familyConfiguration;
        _serverApiService = serverApiService;
        _randomGenerator = randomGenerator;
        _mapManager = mapManager;
        _reputationConfiguration = reputationConfiguration;
        _itemsManager = itemsManager;
        _subActConfiguration = subActConfiguration;
        _rankingManager = rankingManager;
        _expirableLockService = expirableLockService;
        _timeSpaceConfiguration = timeSpaceConfiguration;
        _sessionService = sessionService;
        _itemInstanceFactory = itemInstanceFactory;
        _characterService = characterService;
        _playerEntityFactory = playerEntityFactory;
        _mateEntityFactory = mateEntityFactory;
        _familyManager = familyManager;
        _bcardHandlerContainer = bcardHandlerContainer;
        _questFactory = questFactory;
        _accountService = accountService;
        _evtbConfiguration = evtbConfiguration;
        _familyService = familyService;
        _battlePassConfiguration = battlePassConfiguration;
        _trainerSpecialistPetBookConfiguration = trainerSpecialistPetBookConfiguration;
        _npcMonsterManager = npcMonsterManager;
    }

    public async Task HandleAsync(CharacterLoadEvent e, CancellationToken cancellation)
    {
        await _semaphoreSlim.WaitAsync(cancellation);
        try
        {
            IClientSession session = e.Sender;

            byte? characterSlot = session.SelectedCharacterSlot;

            if (characterSlot is null)
            {
                session.ForceDisconnect();
                return;
            }

            if (session.HasCurrentMapInstance || session.HasSelectedCharacter)
            {
                return;
            }

            session.SelectedCharacterSlot = null;
            DbServerGetCharacterResponse response = await _characterService.GetCharacterBySlot(new DbServerGetCharacterFromSlotRequest
            {
                AccountId = session.Account.Id,
                Slot = characterSlot.Value
            });

            if (response is not { RpcResponseType: RpcResponseType.SUCCESS } || response.CharacterDto is null)
            {
                session.ForceDisconnect();
                return;
            }

            SessionResponse sessionResponse = await _sessionService.GetSessionByAccountId(new GetSessionByAccountIdRequest { AccountId = session.Account.Id });
            if (sessionResponse is not { ResponseType: RpcResponseType.SUCCESS })
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] sessionResponse not success => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            Session storedSession = sessionResponse.Session;
            if (storedSession.ChannelId != _serverManager.ChannelId)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] Wrong session ChannelId => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            if (storedSession.EncryptionKey != session.SessionId)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] Wrong encryption key => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }


            AccountPenaltyGetAllResponse penaltiesResponse = null;
            try
            {
                penaltiesResponse = await _accountService.GetAccountPenalties(new AccountPenaltyGetRequest
                {
                    AccountId = session.Account.Id
                });
            }
            catch (Exception ex)
            {
                Log.Error("[CHARACTER_PRELOAD] Unexpected error: ", ex);
            }

            if (penaltiesResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Error("penaltiesResponse.AccountPenaltyDtos", new Exception());
                session.ForceDisconnect();
                return;
            }

            session.Account.Logs = penaltiesResponse.AccountPenaltyDtos == null ? [] : penaltiesResponse.AccountPenaltyDtos.ToList();

            IPlayerEntity playerEntity = _playerEntityFactory.CreatePlayerEntity(response.CharacterDto);
            
            // playerEntity.EnsurePrestigeInitialized(_prestigeConfiguration);
            //
            // response.CharacterDto.CharacterPrestigeDto ??= new CharacterPrestigeDto
            // {
            //     CurrentPrestigeLevel = playerEntity.CharacterPrestigeDto.CurrentPrestigeLevel,
            //     CurrentPrestigeExp = playerEntity.CharacterPrestigeDto.CurrentPrestigeExp,
            //     Tasks = playerEntity.CharacterPrestigeDto.Tasks?.ToList() ?? new List<PrestigeTaskDto>()
            // };
            //
            // if (playerEntity.CharacterPrestigeDto.Tasks is null || playerEntity.CharacterPrestigeDto.Tasks.Count == 0)
            // {
            //     playerEntity.CharacterPrestigeDto.GenerateTasksForNextLevel(_prestigeConfiguration);
            //     response.CharacterDto.CharacterPrestigeDto.Tasks =
            //         (playerEntity.CharacterPrestigeDto.Tasks ?? new List<PrestigeTaskDto>()).ToList();
            // }
            
            playerEntity.GameStartDate = DateTime.UtcNow;
            playerEntity.MapInstanceId = _mapManager.GetBaseMapInstanceIdByMapId(playerEntity.MapId);
            IMapInstance currentMap = _mapManager.GetMapInstance(playerEntity.MapInstanceId);

            if (currentMap != null && currentMap.IsBlockedZone(playerEntity.MapX, playerEntity.MapY))
            {
                Position pos = currentMap.GetRandomPosition();
                playerEntity.ChangePosition(pos);
            }
            else
            {
                playerEntity.ChangePosition(new Position(playerEntity.MapX, playerEntity.MapY));
            }

            playerEntity.Authority = session.Account.Authority;


            if (response.CharacterDto.ReturnPoint != null)
            {
                if (response.CharacterDto.ReturnPoint.MapId == 10000 && !session.IsGameMaster())
                {
                    response.CharacterDto.ReturnPoint.MapId = 1;
                    response.CharacterDto.ReturnPoint.MapX = 1;
                    response.CharacterDto.ReturnPoint.MapY = 1;
                }
            }

            playerEntity.HomeComponent.ChangeReturn(response.CharacterDto.ReturnPoint);
            playerEntity.HomeComponent.ChangeAct5Respawn(response.CharacterDto.Act5RespawnType);
            playerEntity.HomeComponent.ChangeRespawn(response.CharacterDto.RespawnType);

            LoadQuicklist(playerEntity);
            LoadInventory(playerEntity);
            LoadSkills(playerEntity);
            LoadScripts(playerEntity);
            LoadQuests(playerEntity);
            LoadTitleBCards(playerEntity);

            sessionResponse = await _sessionService.GetSessionByAccountId(new GetSessionByAccountIdRequest { AccountId = session.Account.Id });
            if (sessionResponse is not { ResponseType: RpcResponseType.SUCCESS })
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] 2nd check sessionResponse not success => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            storedSession = sessionResponse.Session;
            if (storedSession.ChannelId != _serverManager.ChannelId)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] 2nd check Wrong session ChannelId => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            if (storedSession.EncryptionKey != session.SessionId)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][LOAD] 2nd check Wrong encryption key => characterName: {response.CharacterDto.Name} | master: {session.Account.MasterAccountId} | hardwareId: {session.HardwareId} on channel {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            IClientSession tmp = _sessionManager.GetSessionByCharacterId(playerEntity.Id);
            if (tmp != null)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER, $"[POTENTIAL_DUPE][LOAD] {playerEntity.Name} tried to connect twice on {_serverManager.ChannelId}!");
                tmp.ForceDisconnect();
                session.ForceDisconnect();
                return;
            }

            // here everything starts
            session.InitializePlayerEntity(playerEntity);

            playerEntity.Miniland ??= _minilandManager.CreateMinilandByCharacterSession(session);

            FamilyMembership familyMember = _familyManager.GetFamilyMembershipByCharacterId(session.PlayerEntity.Id);
            playerEntity.SetFamilyMembership(familyMember);

            foreach (IMateEntity mate in session.PlayerEntity.NosMates
                         .Select(mateDto => _mateEntityFactory.CreateMateEntity(session.PlayerEntity, mateDto))
                         .Where(m => m is not null))
            {
                if (mate.CharacterId != session.PlayerEntity.Id)
                {
                    mate.CharacterId = session.PlayerEntity.Id;
                }

                await session.EmitEventAsync(new MateInitializeEvent
                {
                    MateEntity = mate,
                    IsOnCharacterEnter = true
                });
            }

            CheckNotInitializedStars(playerEntity);
            LoadPartnerWarehouse(playerEntity);
            LoadPartnerInventory(playerEntity);
            LoadPartnerEqBCards(playerEntity);

            await session.EmitEventAsync(new MinigameRefreshProductionEvent(false));
            await session.EmitEventAsync(new QuestDailyRefreshEvent { Force = false });
            await session.EmitEventAsync(new ComplimentsMonthlyRefreshEvent { Force = false });
            await session.EmitEventAsync(new RainbowBattleLeaverBusterRefreshEvent { Force = false });
            await session.EmitEventAsync(new SpecialistRefreshEvent { Force = false });
            await session.EmitEventAsync(new RaidResetRestrictionEvent());
            await session.EmitEventAsync(new RemoveAllBattlePassQuestExpiredEvent());
            await session.EmitEventAsync(new ItemResetRestrictionEvent());
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.LoginXTimeInARow));
            await session.EmitEventAsync(new CharacterSizeModifierCheckEvent());
            
            session.SendPacket("food 0 0");
            session.SendPacket(session.GenerateFoodPacket());
            session.SendPacket(session.GenerateFishPacket(_itemsManager));
            
            session.CurrentMapInstance = session.PlayerEntity.MapInstance;

            // need to improve this logic
            if (await PreLoadLogicAndCheckChannelChange(session))
            {
                Log.Warn($"[CHARACTER_LOAD] {session.PlayerEntity.Name} changed to another channel");
                return;
            }

            IFamily family = session.PlayerEntity.Family;

            await ExecuteInitialMapJoin(session, family);

            await TryToSendSomeWarningMessages(session);

            // todo dump
            _sessionManager.AddOnline(new ClusterCharacterInfo
            {
                ChannelId = (byte?)_gameServer.ChannelId,
                Id = session.PlayerEntity.Id,
                Name = session.PlayerEntity.Name,
                Class = session.PlayerEntity.Class,
                Gender = session.PlayerEntity.Gender,
                HeroLevel = session.PlayerEntity.HeroLevel,
                Level = session.PlayerEntity.Level,
                HardwareId = session.HardwareId
            });

            if (family != null)
            {
                FamilyPacketExtensions.SendFamilyMembersInfoToMembers(family, _sessionManager, _familyConfiguration);
            }

            await _messagePublisher.PublishAsync(new PlayerConnectedOnChannelMessage
            {
                ChannelId = _gameServer.ChannelId,
                CharacterId = session.PlayerEntity.Id,
                CharacterName = session.PlayerEntity.Name,
                Class = session.PlayerEntity.Class,
                Gender = session.PlayerEntity.Gender,
                HeroLevel = session.PlayerEntity.HeroLevel,
                Level = session.PlayerEntity.Level,
                FamilyId = family?.Id,
                HardwareId = session.HardwareId
            }, cancellation);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task<bool> PreLoadLogicAndCheckChannelChange(IClientSession session)
    {
        await RefreshRevivalInfo(session);

        await LoadBuffs(session);

        if (session.CurrentMapInstance != null && !session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return false;
        }

        if (session.PlayerEntity.Faction == FactionType.Neutral)
        {
            return false;
        }

        GetChannelInfoResponse response = null;
        try
        {
            response = await _serverApiService.GetAct4ChannelInfo(new GetAct4ChannelInfoRequest
            {
                WorldGroup = _serverManager.ServerGroup
            });
        }
        catch (Exception e)
        {
            Log.Error("[CHARACTER_LOAD] Unexpected error while trying to obtain channel's Act4 information: ", e);
        }

        SerializableGameServer gameServer = response?.GameServer;

        if (response?.ResponseType != RpcResponseType.SUCCESS || gameServer == null)
        {
            session.ChangeMap(1, 149, 60);
            return false;
        }

        if (gameServer.ChannelId == _gameServer.ChannelId)
        {
            return false;
        }

        short mapId = session.PlayerEntity.Faction == FactionType.Angel ? (short)MapIds.ACT4_ANGEL_CITADEL : (short)MapIds.ACT4_DEMON_CITADEL;
        short mapX = (short)(12 + _randomGenerator.RandomNumber(-2, 3));
        short mapY = (short)(40 + _randomGenerator.RandomNumber(-2, 3));

        if (_mapManager.HasMapFlagByMapId(session.PlayerEntity.MapId, MapFlags.ACT_4))
        {
            mapId = (short)session.PlayerEntity.MapId;
            mapX = session.PlayerEntity.MapX;
            mapY = session.PlayerEntity.MapY;
        }

        await session.EmitEventAsync(new PlayerChangeChannelEvent(gameServer, ItModeType.ToAct4, mapId, mapX, mapY));
        return true;
    }

    private async Task LoadBuffs(IClientSession session)
    {
        var toAdd = new List<Buff>();
        var toRemove = new List<CharacterStaticBuffDto>();
        foreach (CharacterStaticBuffDto buff in session.PlayerEntity.StaticBuffs)
        {
            if (buff.RemainingTime <= 0)
            {
                toRemove.Add(buff);
                continue;
            }

            toAdd.Add(_buffFactory.CreateBuff(buff.CardId, session.PlayerEntity, TimeSpan.FromMilliseconds(buff.RemainingTime), BuffFlag.BIG_AND_KEEP_ON_LOGOUT, true));
        }

        await session.PlayerEntity.AddBuffsAsync(toAdd);
        foreach (CharacterStaticBuffDto removeBuff in toRemove)
        {
            session.PlayerEntity.StaticBuffs.Remove(removeBuff);
        }
    }

    private async Task RefreshRevivalInfo(IClientSession session)
    {
        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.GetMates())
        {
            _revivalManager.TryUnregisterRevival(mate.Id);
        }

        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        await session.EmitEventAsync(new RevivalReviveEvent(RevivalType.DontPayRevival)
        {
            Forced = ForcedType.Reconnect
        });
    }


    private async Task TryToSendSomeWarningMessages(IClientSession session)
    {
        await session.EmitEventAsync(new InventoryExpiredItemsEvent());

        await session.EmitEventAsync(new CharacterBonusExpiredEvent());

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.BazaarMedalGold) || session.PlayerEntity.HaveStaticBonus(StaticBonusType.BazaarMedalSilver))
        {
            session.SendChatMessage(_language.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_ON_LOGIN_BAZAAR_MEDAL, session.UserLanguage), ChatMessageColorType.Green);
        }

        if (!string.IsNullOrWhiteSpace(session.PlayerEntity.Family?.Message))
        {
            session.SendInfo("--- Family Message ---\n" + session.PlayerEntity.Family.Message);
        }

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.GetMates())
        {
            session.SendCondMate(mate);
            if (mate.IsTeamMember)
            {
                await session.EmitEventAsync(new MateJoinTeamEvent { MateEntity = mate, IsOnCharacterEnter = true });
                continue;
            }

            await session.EmitEventAsync(new MateStayInsideMinilandEvent { MateEntity = mate, IsOnCharacterEnter = true });
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.PetBasket))
        {
            session.SendPetBasketPacket(true);
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.AutoLoot))
        {
            session.PlayerEntity.HasAutoLootEnabled = true;
        }
        
        foreach (FamilyBuffCrossChannel familyManagerCurrentFamilyBuff in _familyManager.CurrentFamilyBuffs)
        {
            session.SendPacket(CharacterPacketExtension.GenerateFamilyBuffPacket(familyManagerCurrentFamilyBuff.BuffVnum, (int)(familyManagerCurrentFamilyBuff.EndTime - DateTime.UtcNow).TotalMilliseconds, true));
        }

        if (_gameServer.ChannelType == GameChannelType.ACT_4)
        {
            bool isLosingRep = await _expirableLockService.ExistsTemporaryLock($"game:locks:character:{session.PlayerEntity.Id}:act-4-less-rep");
            if (isLosingRep)
            {
                session.PlayerEntity.IsGettingLosingReputation = true;
                session.SendChatMessage(session.GetLanguage(GameDialogKey.ACT4_CHATMESSAGE_LESS_REPUTATION), ChatMessageColorType.Red);
            }
        }

        AccountPenaltyDto mute = session.Account.Logs.FirstOrDefault(x => x.PenaltyType == PenaltyType.Muted && x.RemainingTime.HasValue);
        if (mute == null)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        session.PlayerEntity.MuteRemainingTime = TimeSpan.FromSeconds(mute.RemainingTime ?? 0);
        session.PlayerEntity.LastChatMuteMessage = DateTime.MinValue;
        session.PlayerEntity.LastMuteTick = now;
    }

    private async Task ExecuteInitialMapJoin(IClientSession session, IFamily family)
    {
        session.SendTitPacket();
        session.RefreshSkillList();

        session.SendRsfiPacket(_subActConfiguration, _timeSpaceConfiguration);

        IReadOnlyList<CharacterDTO> topReputation = _rankingManager.TopReputation;
        session.RefreshReputation(_reputationConfiguration, topReputation);
        session.RefreshPassiveBCards();
        session.SendTitlePacket();
        session.SendIncreaseRange();

        session.ShowInventoryExtensions();
        session.SendStaticBonuses();
        session.SendMinilandPrivateInformation(_minilandManager, _language);

        session.SendTitlePacket();

        session.SendZzimPacket();
        session.SendTwkPacket();

        session.SendPacket(session.GenerateAct6EmptyPacket());

        session.RefreshFaction();
        session.SendMessageUnderChat();

        session.SendStartStartupInventory();

        session.RefreshGold();

        session.SendClinitPacket(_rankingManager.TopCompliment);
        session.SendFlinitPacket(topReputation);
        session.SendKdlinitPacket(_rankingManager.TopPoints);

        session.SendPacket("skyinit 0");
        session.SendPacket("skyinit 1");
        session.SendPacket("skyinit 2");
        session.SendPacket("skyinit 3");
        session.SendPacket("tbf  1.0 2.0 3.0 4.0 5.0");
        
        session.SendStpPacket(false, _trainerSpecialistPetBookConfiguration);
        session.SendStpPacket(true, _trainerSpecialistPetBookConfiguration);
        session.SendStpM();

        session.RefreshSpPoint();
        session.SendPacket("rage 0 250000");
        session.SendPacket("rank_cool 0 0 18000");
        session.SendEvtbPacket(_evtbConfiguration);
        session.SendEmptyMateSkillPacket();
        if (_battlePassConfiguration.IsBattlePassEnabled)
        {
            session.EmitEvent(new BattlePassQuestPacketEvent());
            session.SendPacket(new BptPacket()
            {
                MinutesUntilSeasonEnd = (long)Math.Round((_battlePassConfiguration.EndSeason - DateTime.Now).TotalMinutes)
            });
        }
        
        if (!await _familyManager.CanJoinNewFamilyAsync(session.PlayerEntity.Id))
        {
            string lockKey = $"game:locks:character:{session.PlayerEntity.Id}:join-family";
            
            DateTime? expirationDate = await _expirableLockService.GetLockExpirationAsync(lockKey);

            if (!expirationDate.HasValue)
            {
                return;
            }

            TimeSpan remainingTime = expirationDate.Value - DateTime.UtcNow;
            
            if (remainingTime.TotalMinutes > 0)
            {
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.CannotBeInvited, (byte)remainingTime.TotalMinutes);
            }
        }


        bool changeMap = false;
        if (family != null)
        {
            if ((byte)session.PlayerEntity.Faction != family.Faction)
            {
                await session.EmitEventAsync(new ChangeFactionEvent
                {
                    NewFaction = family.Faction == (byte)FactionType.Angel ? FactionType.Angel : FactionType.Demon
                });

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    changeMap = true;
                }
            }

            session.BroadcastGidx(family, _language);
            session.RefreshFamilyInfo(family, _familyConfiguration);
            session.SendFamilyLogsToMember(family);
            session.RefreshFamilyMembers(_sessionManager, family);
            session.RefreshFamilyMembersMessages(family);
            session.RefreshFamilyMembersExp(family);
            session.SendFmiPacket();
            session.SendFmpPacket(_itemsManager, _familyManager, _rankingManager, _familyConfiguration);
        }

        if (session.PlayerEntity.Level < 85)
        {
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_NO_HERO, session.PlayerEntity));
        }

        if (session.PlayerEntity.HeroLevel != 0 && session.PlayerEntity.HeroLevel < 30)
        {
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_HERO, session.PlayerEntity));
        }

        session.RefreshQuicklist();

        session.ChangeToLastBaseMap();

        session.SendPacket("scr 0 0 0 0 0 0");

        await ExecuteQuestLogic(session);

        if (!changeMap)
        {
            return;
        }

        await session.Respawn();
    }

    private async Task ExecuteQuestLogic(IClientSession session)
    {
        CompletedScriptsDto lastCompletedScriptDto = session.PlayerEntity?.GetLastCompletedScript();
        if (lastCompletedScriptDto == null)
        {
            if (session.PlayerEntity is { Class: ClassType.MartialArtist })
            {
                if (!session.PlayerEntity.CompletedTimeSpaces.Contains(78))
                {
                    foreach (SubActsConfiguration act in _subActConfiguration.GetConfigurations())
                    {
                        if (act.Act > 3)
                        {
                            break;
                        }
                        
                        foreach (long ts in act.TimeSpaces)
                        {
                            session.PlayerEntity.CompletedTimeSpaces.Add(ts);
                        }
                    }
                    
                    
                    session.SendRsfiPacket(_subActConfiguration,_timeSpaceConfiguration);
                }
                
                TutorialDto tutorialDto = _questManager.GetScriptTutorialByIndex(1, 10);
                if (tutorialDto == null)
                {
                    return;
                }

                IReadOnlyCollection<TutorialDto> scripts = _questManager.GetScriptsTutorialUntilIndex(1, 10);
                IEnumerable<CompletedScriptsDto> playerCompletedScripts = session.PlayerEntity.GetCompletedScripts();

                foreach (TutorialDto script in scripts)
                {
                    session.PlayerEntity.SaveScript(script.ScriptId, script.ScriptIndex, script.Type, DateTime.UtcNow);
                    if (script.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION)
                    {
                        session.PlayerEntity.AddCompletedQuest(_questFactory.NewQuest(session.PlayerEntity.Id, script.Data, QuestSlotType.MAIN));
                    }
                }

                foreach (CompletedScriptsDto completedScript in playerCompletedScripts)
                {
                    if (completedScript.ScriptId < 1 || (completedScript.ScriptId == 1 && completedScript.ScriptIndex <= 10))
                    {
                        continue;
                    }

                    session.PlayerEntity.RemoveCompletedScript(completedScript.ScriptId, completedScript.ScriptIndex);

                    TutorialDto script = _questManager.GetScriptTutorialByIndex(completedScript.ScriptId, completedScript.ScriptIndex);
                    if (script.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION)
                    {
                        session.PlayerEntity.RemoveCompletedQuest(script.Data);
                    }
                }

                TutorialDto nextScript2 = _questManager.GetScriptTutorialById(scripts.Last().Id + 1);
                if (nextScript2 == null)
                {
                    return;
                }

                CharacterQuest characterQuest = session.GetQuestBySlot(5);
                if (characterQuest != null) // Remove a MAIN quest if there is any
                {
                    session.DeleteQuestTarget(characterQuest);
                    session.PlayerEntity.RemoveActiveQuest(characterQuest.QuestId);
                    session.RefreshQuestList(_questManager, null);
                }

                if (nextScript2.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && session.PlayerEntity.HasQuestWithId(nextScript2.Data))
                {
                    QuestDto quest = _questManager.GetQuestById(nextScript2.Data);
                    session.PlayerEntity.AddActiveQuest(_questFactory.NewQuest(session.PlayerEntity.Id, nextScript2.Data, QuestSlotType.MAIN));
                    session.RefreshQuestList(_questManager, quest.Id);
                    session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
                }

                session.SendScriptPacket(nextScript2.ScriptId, nextScript2.ScriptIndex);
                return;
            }
            
            if (session.PlayerEntity != null && !session.PlayerEntity.CompletedTimeSpaces.Contains(78))
            {
                foreach (SubActsConfiguration act in _subActConfiguration.GetConfigurations())
                {
                    if (act.Act > 3)
                    {
                        break;
                    }
                    
                    foreach (long ts in act.TimeSpaces)
                    {
                        session.PlayerEntity.CompletedTimeSpaces.Add(ts);
                    }
                }
                
                
                session.SendRsfiPacket(_subActConfiguration,_timeSpaceConfiguration);
            }

            session.SendScriptPacket(1, 10);
            return;
        }
        
        TutorialDto lastCompletedTutorial = _questManager.GetScriptTutorialByIndex(lastCompletedScriptDto.ScriptId, lastCompletedScriptDto.ScriptIndex);

        if (lastCompletedTutorial == null)
        {
            return;
        }

        TutorialDto nextScript = _questManager.GetScriptTutorialById(lastCompletedTutorial.Id + 1);

        // We check if it was the last script from that sub-questline, for sending a QnpcPacket instead of a new script
        if (lastCompletedTutorial.ScriptIndex == _questManager.GetScriptsTutorialByScriptId(lastCompletedTutorial.ScriptId).Max(s => s.ScriptIndex))
        {
            QuestNpcDto questNpc = _questManager.GetQuestNpcByScriptId(lastCompletedTutorial.ScriptId + 1);
            if (questNpc != null)
            {
                session.SendQnpcPacket(questNpc.Level, questNpc.NpcVnum, questNpc.MapId);
            }
            await CheckCompletedQuests(session);
            return;
        }

        // We check if we have an active quest-script
        CompletedScriptsDto lastCompletedQuestStartScript = session.PlayerEntity?.GetLastCompletedScriptByType(TutorialActionType.START_QUEST);
        if (lastCompletedQuestStartScript != null)
        {
            TutorialDto lastCompletedQuestStartTutorial = _questManager.GetScriptTutorialByIndex(lastCompletedQuestStartScript.ScriptId, lastCompletedQuestStartScript.ScriptIndex);
            TutorialDto waitQuestCompletionScript = _questManager.GetScriptsTutorialByType(TutorialActionType.WAIT_FOR_QUEST_COMPLETION)
                .First(s => s.Data == lastCompletedQuestStartTutorial.Data && s.Id > lastCompletedQuestStartTutorial.Id);
            if (!session.PlayerEntity.HasCompletedScriptByIndex(waitQuestCompletionScript.ScriptId, waitQuestCompletionScript.ScriptIndex))
            {
                if (!session.PlayerEntity.HasQuestWithId(lastCompletedQuestStartTutorial.Data))
                {
                    session.PlayerEntity.RemoveCompletedScript(lastCompletedQuestStartTutorial.ScriptId, lastCompletedQuestStartTutorial.ScriptIndex);
                    var completedScriptsToRemove = session.PlayerEntity.GetCompletedScripts().Where(s => s.ScriptId > lastCompletedQuestStartTutorial.ScriptId ||
                        s.ScriptId == lastCompletedQuestStartTutorial.ScriptId && s.ScriptIndex > lastCompletedQuestStartTutorial.ScriptIndex).ToList();
                    foreach (CompletedScriptsDto completedScript in completedScriptsToRemove)
                    {
                        session.PlayerEntity.RemoveCompletedScript(completedScript.ScriptId, completedScript.ScriptIndex);
                    }

                    nextScript = lastCompletedQuestStartTutorial;
                }
                else
                {
                    nextScript = waitQuestCompletionScript;
                }
            }
        }

        session.SendScriptPacket(nextScript.ScriptId, nextScript.ScriptIndex);
        await CheckCompletedQuests(session);
    }


    private async Task CheckCompletedQuests(IClientSession session)
    {
        foreach (CharacterQuest characterQuest in session.PlayerEntity.GetCurrentQuests())
        {
            if (session.PlayerEntity.IsQuestCompleted(characterQuest))
            {
                await session.EmitEventAsync(new QuestCompletedEvent(characterQuest, true));
            }
        }

        session.RefreshQuestList(_questManager, null);
        session.SendQuestsTargets();
        session.SendSqstPackets(_questManager);
    }

    private void LoadSkills(IPlayerEntity session)
    {
        foreach (CharacterSkillDTO skillDto in session.LearnedSkills)
        {
            if (session.CharacterSkills.ContainsKey(skillDto.SkillVNum))
            {
                continue;
            }

            var skill = new CharacterSkill
            {
                SkillVNum = skillDto.SkillVNum,
                UpgradeSkill = skillDto.UpgradeSkill,
            };

            session.CharacterSkills[skill.SkillVNum] = skill;
            session.Skills.Add(skill);

            short upgradeSkill = skill.Skill?.UpgradeSkill ?? 0;

            if (upgradeSkill == 0)
            {
                continue;
            }

            if (!session.SkillComponent.SkillUpgrades.TryGetValue(upgradeSkill, out HashSet<IBattleEntitySkill> hashSet))
            {
                hashSet = new HashSet<IBattleEntitySkill>();
                session.SkillComponent.SkillUpgrades[upgradeSkill] = hashSet;
            }

            if (hashSet.Contains(skill))
            {
                continue;
            }

            hashSet.Add(skill);
        }
    }

    private void LoadScripts(IPlayerEntity session)
    {
        IEnumerable<CompletedScriptsDto> completedScriptsDtos = session.CompletedScripts;
        foreach (CompletedScriptsDto completedScriptDto in completedScriptsDtos)
        {
            TutorialDto completedScript = _questManager.GetScriptTutorialByIndex(completedScriptDto.ScriptId, completedScriptDto.ScriptIndex);

            if (completedScript == null)
            {
                continue;
            }

            session.SaveScript(completedScript.ScriptId, completedScript.ScriptIndex, completedScript.Type, completedScriptDto.CompletedDate);
        }
    }


    private void LoadQuests(IPlayerEntity session)
    {
        IEnumerable<CharacterQuestDto> characterQuests = session.ActiveQuests;
        IEnumerable<CharacterQuestDto> completedPeriodicQuests = session.CompletedPeriodicQuests;
        IEnumerable<CharacterQuestDto> completedQuests = session.CompletedQuests;

        foreach (CharacterQuestDto characterQuestDto in completedPeriodicQuests)
        {
            CharacterQuest characterQuest = _questFactory.NewQuest(session.Id, characterQuestDto.QuestId, characterQuestDto.SlotType);
            characterQuest.ObjectiveAmount = characterQuestDto.ObjectiveAmount;
            session.AddCompletedPeriodicQuest(characterQuest);
        }

        // This can be removed when the wipe of the server happens.
        // It's gonna contain only the MAIN quests, since we have a "backup" (the scripts), but 
        // with new characters will also contain GENERAL and SECONDARY ones
        if (!completedQuests.Any())
        {
            foreach (CompletedScriptsDto completedScript in session.GetCompletedScriptsByType(TutorialActionType.START_QUEST))
            {
                TutorialDto script = _questManager.GetScriptTutorialByIndex(completedScript.ScriptId, completedScript.ScriptIndex);

                if (script == null)
                {
                    continue;
                }

                if (characterQuests.Any(s => s.QuestId == script.Data)) // It's an active one
                {
                    continue;
                }

                CharacterQuest characterQuest = _questFactory.NewQuest(session.Id, script.Data, QuestSlotType.MAIN);
                session.AddCompletedQuest(characterQuest);
            }
        }

        else
        {
            foreach (CharacterQuestDto characterQuestDto in completedQuests)
            {
                CharacterQuest characterQuest = _questFactory?.NewQuest(session.Id, characterQuestDto.QuestId, characterQuestDto.SlotType);
                if (characterQuest == null)
                    continue;
                characterQuest.ObjectiveAmount = characterQuestDto.ObjectiveAmount;
                session.AddCompletedQuest(characterQuest);
            }
        }

        foreach (CharacterQuestDto characterQuestDto in characterQuests)
        {
            if (_questFactory != null)
            {
                CharacterQuest characterQuest = _questFactory.NewQuest(session.Id, characterQuestDto.QuestId, characterQuestDto.SlotType);
                characterQuest.ObjectiveAmount = characterQuestDto.ObjectiveAmount;
                session.AddActiveQuest(characterQuest);
            }
        }
    }
    
    private void CheckNotInitializedStars(IPlayerEntity playerEntity)
    {
        foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.MateType == MateType.Pet))
        {
            if (mate.Stars != 0)
            {
                continue;
            }

            IMonsterData monsterData = _npcMonsterManager.GetNpc(mate.MonsterVNum);
            mate.Stars = monsterData.Stars;
        }
    }

    private void LoadPartnerEqBCards(IPlayerEntity playerEntity)
    {
        foreach (IMateEntity mate in playerEntity.MateComponent.GetMates(x => x.MateType == MateType.Partner))
        {
            foreach (PartnerInventoryItem partnerInventory in playerEntity.PartnerGetEquippedItems(mate.PetSlot))
            {
                if (partnerInventory?.ItemInstance == null)
                {
                    continue;
                }

                mate.RefreshEquipmentValues(partnerInventory.ItemInstance, false);
            }
        }
    }

    private void LoadTitleBCards(IPlayerEntity playerEntity)
    {
        CharacterTitleDto equippedTitle = playerEntity.Titles.FirstOrDefault(x => x.IsEquipped);
        if (equippedTitle == null)
        {
            return;
        }

        playerEntity.RefreshTitleBCards(_itemsManager, equippedTitle, _bcardHandlerContainer);
    }

    private void LoadPartnerInventory(IPlayerEntity playerEntity)
    {
        IEnumerable<CharacterPartnerInventoryItemDto> partnerItems = playerEntity.PartnerInventory;
        foreach (CharacterPartnerInventoryItemDto partnerItem in partnerItems)
        {
            if (partnerItem == null)
            {
                continue;
            }

            GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(partnerItem.ItemInstance);
            playerEntity.PartnerEquipItem(itemInstance, partnerItem.PartnerSlot);
        }
    }

    private void LoadQuicklist(IPlayerEntity playerEntity)
    {
        List<CharacterQuicklistEntryDto> inventory = playerEntity.Quicklist;
        foreach (CharacterQuicklistEntryDto item in inventory)
        {
            if (item == null)
            {
                continue;
            }

            playerEntity.QuicklistComponent.AddQuicklist(item);
        }
    }

    private void LoadInventory(IPlayerEntity playerEntity)
    {
        foreach (CharacterInventoryItemDto item in playerEntity.Inventory)
        {
            if (item == null)
            {
                continue;
            }

            CharacterInventoryItemDto inventoryItem = item;

            GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(inventoryItem.ItemInstance);
            var newItem = new InventoryItem
            {
                CharacterId = inventoryItem.CharacterId,
                InventoryType = inventoryItem.InventoryType,
                IsEquipped = inventoryItem.IsEquipped,
                Slot = inventoryItem.Slot,
                ItemInstance = itemInstance
            };

            if (newItem.InventoryType == InventoryType.EquippedItems && newItem.ItemInstance != null)
            {
                playerEntity.EquipItem(newItem, newItem.ItemInstance.GameItem.EquipmentSlot, true);
                if (itemInstance.Type == ItemInstanceType.WearableInstance)
                {
                    playerEntity.RefreshEquipmentValues(itemInstance);
                }

                continue;
            }

            playerEntity.AddItemToInventory(newItem);
        }

        // equipped items
        DateTime date = DateTime.UtcNow;
        foreach (CharacterInventoryItemDto item in playerEntity.EquippedStuffs)
        {
            if (item == null)
            {
                continue;
            }

            GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(item.ItemInstance);
            var newItem = new InventoryItem
            {
                CharacterId = item.CharacterId,
                InventoryType = item.InventoryType,
                IsEquipped = item.IsEquipped,
                Slot = item.Slot,
                ItemInstance = itemInstance
            };
            if (newItem.InventoryType != InventoryType.EquippedItems || newItem.ItemInstance == null)
            {
                continue;
            }

            if (newItem.ItemInstance.ItemDeleteTime <= date)
            {
                continue;
            }

            playerEntity.EquipItem(newItem, newItem.ItemInstance.GameItem.EquipmentSlot, true);
            if (itemInstance.Type == ItemInstanceType.WearableInstance)
            {
                playerEntity.RefreshEquipmentValues(itemInstance);
            }
        }
    }

    private void LoadPartnerWarehouse(IPlayerEntity playerEntity)
    {
        IEnumerable<PartnerWarehouseItemDto> items = playerEntity.PartnerWarehouse;
        foreach (PartnerWarehouseItemDto item in items)
        {
            if (item == null)
            {
                continue;
            }

            playerEntity.AddPartnerWarehouseItem(_itemInstanceFactory.CreateItem(item.ItemInstance), item.Slot);
        }
    }
}