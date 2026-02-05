using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Drops;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Gold;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Prestige;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.PrivateMapInstances.Events;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Raids;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public sealed class KillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
{
    private readonly IDropManager _dropManager;
    private readonly IDropRarityConfigurationProvider _dropRarityConfigurationProvider;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private static readonly Dictionary<long, Timer> ExpTimers = new();
    private static readonly Dictionary<long, int> ExpAccumulators = new();
    private readonly GeneralMapGoldConfiguration _generalMapGoldConfiguration;
    
    private readonly HashSet<QuestType> NormalDropQuestTypes = new() { QuestType.DROP_CHANCE, QuestType.DROP_CHANCE_2, QuestType.DROP_IN_TIMESPACE };

    public KillBonusEventHandler(IRandomGenerator randomGenerator,
        IDropManager dropManager, IServerManager serverManager, IGameLanguageService gameLanguage, ISessionManager sessionManager,
        IItemsManager itemsManager, IAsyncEventPipeline eventPipeline, IGameItemInstanceFactory gameItemInstance,
        IDropRarityConfigurationProvider dropRarityConfigurationProvider, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager,
        BattlePassQuestConfiguration battlePassQuestConfiguration, IEvtbConfiguration evtbConfiguration,
        GeneralMapGoldConfiguration generalMapGoldConfiguration)
    {
        _randomGenerator = randomGenerator;
        _dropManager = dropManager;
        _serverManager = serverManager;
        _gameLanguage = gameLanguage;
        _sessionManager = sessionManager;
        _itemsManager = itemsManager;
        _eventPipeline = eventPipeline;
        _gameItemInstance = gameItemInstance;
        _dropRarityConfigurationProvider = dropRarityConfigurationProvider;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
        _evtbConfiguration = evtbConfiguration;
        _generalMapGoldConfiguration = generalMapGoldConfiguration;
    }

    public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToAttack = e.MonsterEntity;
        IPlayerEntity character = e.Sender.PlayerEntity;
        IClientSession session = e.Sender;

        if (monsterEntityToAttack == null
            || (monsterEntityToAttack.IsStillAlive && !e.IsByButcherCommand)
            || monsterEntityToAttack.SummonerType is VisualType.Player)
        {
            return;
        }


        if (!ShouldMonsterDrop(monsterEntityToAttack))
        {
            return;
        }

        // owner set
        IPlayerEntity dropOwner = null;

        if (monsterEntityToAttack.Damagers.Count > 0)
        {
            IBattleEntity entityDropOwner = monsterEntityToAttack.Damagers.FirstOrDefault();
            if (entityDropOwner != null)
            {
                dropOwner = entityDropOwner switch
                {
                    IMonsterEntity monsterEntity
                        => monsterEntity.SummonerType != null && monsterEntity.SummonerId != null && monsterEntity.SummonerType == VisualType.Player
                            ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                            : null,
                    IPlayerEntity playerEntity => playerEntity,
                    IMateEntity mateEntity => mateEntity.Owner,
                    _ => null
                };
            }
        }

        // Check if owner is online and it's at the same map
        IClientSession firstAttacker = dropOwner != null ? _sessionManager.GetSessionByCharacterId(dropOwner.Id) : null;
        if (firstAttacker == null)
        {
            dropOwner = character;
        }
        else
        {
            dropOwner = firstAttacker.CurrentMapInstance?.Id == character.MapInstance.Id ? firstAttacker.PlayerEntity : character;
        }

        PlayerGroup playerGroup = null;
        if (dropOwner != null)
        {
            playerGroup = dropOwner.GetGroup();
        }

        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.TierLevel <= 4)
        {
            await HandleTierExperience(session, monsterEntityToAttack);
        }

        await HandleBattlePassMission(session, monsterEntityToAttack);
        await HandlePrestigeMonsterKillMission(session, monsterEntityToAttack, cancellation);
        await HandleExp(session, character, monsterEntityToAttack, dropOwner?.Id);
        await HandleGoldDrops(monsterEntityToAttack, playerGroup, dropOwner, e.IsByButcherCommand);
        await HandleDrops(monsterEntityToAttack, session.PlayerEntity, playerGroup, dropOwner, e.IsByButcherCommand);
    }

    private async Task HandlePrestigeMonsterKillMission(IClientSession session, IMonsterEntity monsterEntity, CancellationToken cancellation)
    {
        int level = session.PlayerEntity.Level;
        int monsterVnum = monsterEntity.MonsterVNum;

        if (monsterEntity.Level >= level - 10 && monsterEntity.Level <= level + 10)
        {
            await _eventPipeline.ProcessEventAsync(new PrestigeProgressEvent(session, PrestigeTaskType.KILL_MONSTERS_BY_LEVEL, amount: 1), cancellation);
        }

        await _eventPipeline.ProcessEventAsync(new PrestigeProgressEvent(session, PrestigeTaskType.KILL_MONSTERS_BY_VNUM, monsterVnum: monsterVnum, amount: 1), cancellation);
        await _eventPipeline.ProcessEventAsync(new PrestigeProgressEvent(session, PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM, monsterVnum: monsterVnum, amount: 1), cancellation);
    }

    private async Task HandleTierExperience(IClientSession session, IMonsterEntity monsterEntityToAttack)
    {
        if (monsterEntityToAttack == null || session?.PlayerEntity == null)
        {
            return;
        }

        if (monsterEntityToAttack.Level >= session.PlayerEntity.Level - 10 && monsterEntityToAttack.Level <= session.PlayerEntity.Level + 10)
        {
            int baseExperience = session.PlayerEntity.SubClass.IsPveSubClass() ? 2 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 1 : 0;
            ExpAccumulators.TryAdd(session.PlayerEntity.Id, 0);
            session.AddTierExperience(baseExperience, _gameLanguage);
        }
    }

    private async Task HandleBattlePassMission(IClientSession session, IMonsterEntity monsterEntityToAttack)
    {
        if (monsterEntityToAttack.Level >= session.PlayerEntity.Level - 10 && monsterEntityToAttack.Level <= session.PlayerEntity.Level + 10)
        {
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.DefeatXMonsterInRange));
        }

        if (monsterEntityToAttack.DropToInventory)
        {
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.DefeatXBossMap));
        }

        if (monsterEntityToAttack.MonsterVNum is 434 or 435 or 436)
        {
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.DefeatXCurserMob));
        }

        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.DefeatXRaceTypeMonsterTime, firstData: monsterEntityToAttack.MonsterRaceSubType));
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.DefeatXMonsterVnum, firstData: monsterEntityToAttack.MonsterVNum));
    }

    private bool ShouldMonsterDrop(IMonsterEntity monsterEntityToAttack)
    {
        switch ((MonsterVnum)monsterEntityToAttack.MonsterVNum)
        {
            case MonsterVnum.TRAINING_STAKE:
            case MonsterVnum.DEMON_CAMP:
            case MonsterVnum.ANGEL_CAMP:
                return false;
        }

        return true;
    }

    private float CalculateDropPenalty(int playerLevel, int monsterLevel, bool isGold)
    {
        int levelDiff = playerLevel - monsterLevel;

        if (levelDiff <= 0)
        {
            return 1.0f;
        }

        if (isGold)
        {
            return levelDiff switch
            {
                <= 14 => 1.0f,            
                >= 15 and <= 24 => 0.5f,  
                _ => 0.05f           
            };
        }

        return levelDiff switch
        {
            >= 20 => 0.1f,
            >= 10 => 0.5f,
            _ => 1.0f
        };
    }

    private async Task HandleExp(IClientSession session, IPlayerEntity character, IMonsterEntity monsterEntityToAttack, long? dropOwner)
    {
        if (!character.IsAlive())
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new GenerateExperienceEvent(character, monsterEntityToAttack, dropOwner));

        if (character.Level >= monsterEntityToAttack.Level || character.Dignity >= 100)
        {
            return;
        }

        character.Dignity += 1;
        session.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);
        session.SendSuccessChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.DIGNITY_CHATMESSAGE_RESTORE, session.UserLanguage, 1));
    }

    private async Task HandleDrops(IMonsterEntity monsterEntityToAttack, IPlayerEntity mainKiller, PlayerGroup playerGroup, IPlayerEntity firstAttacker, bool isByButcherCommand)
    {
        IClientSession session = firstAttacker.Session;

        IReadOnlyList<DropDTO> monsterDrops = monsterEntityToAttack.Drops;
        var additionalDrop = new List<DropDTO>();
        IReadOnlyList<DropDTO> mapDrop = _dropManager.GetDropsByMapId(monsterEntityToAttack.MapInstance.MapId);
        IEnumerable<DropDTO> generalDrop = _dropManager.GetGeneralDrops();
        additionalDrop.AddRange(mapDrop);
        additionalDrop.AddRange(generalDrop);

        int secondChanceDropBCard = session.PlayerEntity.BCardComponent
            .GetAllBCardsInformation(BCardType.DropItemTwice, (byte)AdditionalTypes.DropItemTwice.DoubleDropChance, session.PlayerEntity.Level).firstData;
        bool secondChanceDrop = secondChanceDropBCard != 0 && _randomGenerator.RandomNumber() <= secondChanceDropBCard;

        #region Quests

        // Normal quest drops
        IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByTypes(NormalDropQuestTypes);
        foreach (CharacterQuest characterQuest in characterQuests)
        {
            foreach (QuestObjectiveDto objective in characterQuest.Quest.Objectives)
            {
                if (monsterEntityToAttack.MonsterVNum != objective.Data0 && characterQuest.Quest.QuestType != QuestType.DROP_IN_TIMESPACE)
                {
                    continue;
                }

                if (characterQuest.Quest.QuestType == QuestType.DROP_IN_TIMESPACE)
                {
                    TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
                    if (timeSpace == null || timeSpace.TimeSpaceId != objective.Data0)
                    {
                        continue;
                    }
                }

                float rndChance = _randomGenerator.RandomNumber();
                float chance = characterQuest.Quest.QuestType == QuestType.DROP_CHANCE ? objective.Data3 : objective.Data3 * 0.1f;
                if (rndChance > chance)
                {
                    continue;
                }

                await DropQuestItem(session, monsterEntityToAttack, playerGroup, objective.Data1, isByButcherCommand);

                if (!secondChanceDrop)
                {
                    continue;
                }

                await DropQuestItem(session, monsterEntityToAttack, playerGroup, objective.Data1, isByButcherCommand);
            }
        }

        // It has to be hardcoded, sorry T-T
        if (session.PlayerEntity.HasQuestWithId((int)QuestsVnums.LILIES_SP2))
        {
            if (monsterEntityToAttack.Level >= session.PlayerEntity.Level - 15 && monsterEntityToAttack.Level <= session.PlayerEntity.Level + 15 || monsterEntityToAttack.Level > 75)
            {
                float rndChance = _randomGenerator.RandomNumber();
                float chance = 25; // It has to be like this for now
                if (rndChance < chance)
                {
                    await DropQuestItem(session, monsterEntityToAttack, playerGroup, (int)ItemVnums.LILY_OF_PURITY, isByButcherCommand);

                    if (secondChanceDrop)
                    {
                        await DropQuestItem(session, monsterEntityToAttack, playerGroup, (int)ItemVnums.LILY_OF_PURITY, isByButcherCommand);
                    }
                }
            }
        }

        #endregion

        int eventIncreaseDropItem = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_ITEM_DROP_CHANCE);
        double dropMultiplier = eventIncreaseDropItem;

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.IS_ACT4_DUNGEON))
        {
            dropMultiplier += session.PlayerEntity.BCardComponent
                .GetAllBCardsInformation(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.DropRateIncrease, session.PlayerEntity.Level).firstData;
            dropMultiplier -= session.PlayerEntity.BCardComponent
                .GetAllBCardsInformation(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.DropRateDecrease, session.PlayerEntity.Level).firstData;
        }

        int rate = (int)(session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) || session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED) ||
            session.PlayerEntity.HasAutoLootEnabled || monsterEntityToAttack.DropToInventory
                ? 1
                : _serverManager.MobDropRate * dropMultiplier);

        if (secondChanceDrop)
        {
            session.PlayerEntity.BroadcastEffectInRange(EffectType.DoubleChanceDrop);
        }

        for (int i = 0; i < rate; i++)
        {
            foreach (DropDTO drop in monsterDrops)
            {
                float rndChance = _randomGenerator.RandomNumber(0, 100000);

                float penalty = CalculateDropPenalty(session.PlayerEntity.Level, monsterEntityToAttack.Level, isGold: false);

                float chance = drop.DropChance
                    * _serverManager.MobDropChance
                    * eventIncreaseDropItem
                    * penalty;

                if (rndChance > chance)
                {
                    continue;
                }

                await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);

                if (secondChanceDrop)
                {
                    await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);
                }
            }
        }

        if (monsterEntityToAttack.RaidDrop != null)
        {
            foreach (DropChance drop in monsterEntityToAttack.RaidDrop)
            {
                float rndChance = _randomGenerator.RandomNumber(0, 100000);

                float penalty = CalculateDropPenalty(session.PlayerEntity.Level, monsterEntityToAttack.Level, isGold: false);

                float chance = drop.Chance
                    * _serverManager.MobDropChance
                    * eventIncreaseDropItem
                    * penalty;

                if (rndChance > chance)
                {
                    continue;
                }

                await DropItem(session, monsterEntityToAttack, drop.ItemVnum, drop.Amount, playerGroup, isByButcherCommand);

                if (secondChanceDrop)
                {
                    await DropItem(session, monsterEntityToAttack, drop.ItemVnum, drop.Amount, playerGroup, isByButcherCommand);
                }
            }
        }

        if (session.PlayerEntity.TimeSpaceComponent.TimeSpace != null
            && monsterEntityToAttack.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
            float rndChance = _randomGenerator.RandomNumber(0, 100000);

            int itemChance = timeSpace.Instance.BonusPointItemDropChance;

            float penalty = CalculateDropPenalty(session.PlayerEntity.Level, monsterEntityToAttack.Level, isGold: false);

            float chance = itemChance
                * _serverManager.MobDropChance
                * eventIncreaseDropItem
                * penalty;

            if (rndChance <= chance)
            {
                await DropItem(session, monsterEntityToAttack, (short)ItemVnums.BONUS_POINTS, 1, playerGroup, isByButcherCommand);

                if (secondChanceDrop)
                {
                    await DropItem(session, monsterEntityToAttack, (short)ItemVnums.BONUS_POINTS, 1, playerGroup, isByButcherCommand);
                }
            }
        }

        IReadOnlyList<DropDTO> raceDrop = _dropManager.GetDropsByMonsterRace(monsterEntityToAttack.MonsterRaceType, monsterEntityToAttack.MonsterRaceSubType);
        for (int i = 0; i < rate; i++)
        {
            foreach (DropDTO drop in raceDrop)
            {
                float rndChance = _randomGenerator.RandomNumber(0, 100000);

                float penalty = CalculateDropPenalty(session.PlayerEntity.Level, monsterEntityToAttack.Level, isGold: false);

                float chance = drop.DropChance
                    * _serverManager.MobDropChance
                    * eventIncreaseDropItem
                    * penalty;

                if (rndChance > chance)
                {
                    continue;
                }

                await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);

                if (secondChanceDrop)
                {
                    await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);
                }
            }
        }

        if (monsterEntityToAttack.MonsterRaceType is MonsterRaceType.Fixed or MonsterRaceType.Other)
        {
            return;
        }

        int genericRate = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) ? 1 : _serverManager.GenericDropRate;

        for (int i = 0; i < genericRate; i++)
        {
            foreach (DropDTO drop in additionalDrop)
            {
                float rndChance = _randomGenerator.RandomNumber(0, 10000);

                float penalty = CalculateDropPenalty(session.PlayerEntity.Level, monsterEntityToAttack.Level, isGold: false);

                float chance = drop.DropChance
                    * _serverManager.GenericDropChance
                    * eventIncreaseDropItem
                    * penalty;

                if (rndChance > chance)
                {
                    continue;
                }

                await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);

                if (secondChanceDrop)
                {
                    await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup, isByButcherCommand);
                }
            }
        }
    }

    private async Task DropQuestItem(IClientSession session, IMonsterEntity monsterEntityToAttack, PlayerGroup playerGroup, int itemVnum, bool isByButcherCommand)
    {
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) || monsterEntityToAttack.DropToInventory
            || session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED) || session.PlayerEntity.HasAutoLootEnabled || isByButcherCommand)
        {
            var alreadyGifted = new List<long>();
            foreach (IBattleEntity entity in monsterEntityToAttack.Damagers)
            {
                long charId = entity.Id;
                if (alreadyGifted.Contains(charId))
                {
                    continue;
                }

                IClientSession giftSession = _sessionManager.GetSessionByCharacterId(charId);
                if (giftSession == null)
                {
                    continue;
                }

                if (giftSession.PlayerEntity.MapInstance?.Id != monsterEntityToAttack.MapInstance?.Id)
                {
                    continue;
                }

                bool shouldReceiveDrop = ShouldReceiveDrop(giftSession, monsterEntityToAttack);
                if (!shouldReceiveDrop)
                {
                    continue;
                }

                if (giftSession.PlayerEntity.IsInGroup())
                {
                    foreach (IPlayerEntity member in giftSession.PlayerEntity.GetGroup().Members)
                    {
                        await member.Session.EmitEventAsync(new QuestItemPickUpEvent
                        {
                            ItemVnum = itemVnum,
                            Amount = 1,
                            SendMessage = true
                        });
                        alreadyGifted.Add(member.Id);
                    }
                }
                else
                {
                    await giftSession.EmitEventAsync(new QuestItemPickUpEvent
                    {
                        ItemVnum = itemVnum,
                        Amount = 1,
                        SendMessage = true
                    });
                    alreadyGifted.Add(giftSession.PlayerEntity.Id);
                }
            }

            return;
        }

        short newX = (short)(monsterEntityToAttack.PositionX + _randomGenerator.RandomNumber(-1, 2));
        short newY = (short)(monsterEntityToAttack.PositionY + _randomGenerator.RandomNumber(-1, 2));

        if (monsterEntityToAttack.MapInstance.IsBlockedZone(newX, newY))
        {
            newX = monsterEntityToAttack.PositionX;
            newY = monsterEntityToAttack.PositionY;
        }

        var newItemPosition = new Position(newX, newY);

        if (playerGroup == null)
        {
            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, 1, ownerId: session.PlayerEntity.Id, isQuest: true);
            await _eventPipeline.ProcessEventAsync(dropItem);
            return;
        }

        string itemName;

        if (playerGroup.SharingMode == (byte)GroupSharingType.ByOrder)
        {
            long? dropOwner = playerGroup.GetNextOrderedCharacterId(session.PlayerEntity);

            if (!dropOwner.HasValue)
            {
                return;
            }

            foreach (IPlayerEntity s in playerGroup.Members)
            {
                itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_ORDERED, session.UserLanguage, 1,
                    itemName, playerGroup.Members.FirstOrDefault(c => c.Id == (long)dropOwner)?.Name), ChatMessageColorType.Yellow);
            }

            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, 1, ownerId: dropOwner.Value, isQuest: true);
            await _eventPipeline.ProcessEventAsync(dropItem);
        }
        else
        {
            foreach (IPlayerEntity s in playerGroup.Members)
            {
                itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_SHARED, session.UserLanguage, 1, itemName), ChatMessageColorType.Yellow);
            }

            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, 1, ownerId: session.PlayerEntity.Id, isQuest: true);
            await _eventPipeline.ProcessEventAsync(dropItem);
        }
    }

    private async Task DropItem(IClientSession session, IMonsterEntity monsterEntityToAttack, int itemVnum, int amount, PlayerGroup playerGroup, bool isByButcherCommand)
    {
        IGameItem item = _itemsManager.GetItem(itemVnum);
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4)
            || session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED)
            || monsterEntityToAttack.DropToInventory
            || (session.PlayerEntity.HasAutoLootEnabled && item?.ItemType != ItemType.Map)
            || isByButcherCommand)
        {
            var alreadyGifted = new HashSet<long>();
            foreach (IBattleEntity entity in monsterEntityToAttack.Damagers)
            {
                long charId = entity.Id;
                if (alreadyGifted.Contains(charId))
                {
                    continue;
                }

                IClientSession giftSession = _sessionManager.GetSessionByCharacterId(charId);

                if (giftSession == null)
                {
                    continue;
                }

                if (giftSession.PlayerEntity.MapInstance?.Id != monsterEntityToAttack.MapInstance?.Id)
                {
                    continue;
                }

                bool shouldReceiveDrop = ShouldReceiveDrop(giftSession, monsterEntityToAttack);
                if (!shouldReceiveDrop)
                {
                    continue;
                }

                if (item != null)
                {
                    sbyte randomRarity = _dropRarityConfigurationProvider.GetRandomRarity(item.ItemType);

                    GameItemInstance itemInstance = _gameItemInstance.CreateItem(itemVnum, amount, 0, randomRarity);
                    await giftSession.AddNewItemToInventory(itemInstance, true, ChatMessageColorType.Yellow, true);
                }

                alreadyGifted.Add(charId);
            }

            return;
        }

        short newX = (short)(monsterEntityToAttack.PositionX + _randomGenerator.RandomNumber(-1, 2));
        short newY = (short)(monsterEntityToAttack.PositionY + _randomGenerator.RandomNumber(-1, 2));

        if (monsterEntityToAttack.MapInstance.IsBlockedZone(newX, newY))
        {
            newX = monsterEntityToAttack.PositionX;
            newY = monsterEntityToAttack.PositionY;
        }

        var newItemPosition = new Position(newX, newY);

        if (playerGroup == null)
        {
            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: session.PlayerEntity.Id);
            await _eventPipeline.ProcessEventAsync(dropItem);
            return;
        }

        if (playerGroup.SharingMode == (byte)GroupSharingType.ByOrder)
        {
            long? dropOwner = playerGroup.GetNextOrderedCharacterId(session.PlayerEntity);

            if (!dropOwner.HasValue)
            {
                return;
            }

            foreach (IPlayerEntity s in playerGroup.Members)
            {
                string itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_ORDERED, s.Session.UserLanguage, amount,
                    itemName, playerGroup.Members.FirstOrDefault(c => c.Id == (long)dropOwner)?.Name), ChatMessageColorType.Yellow);
            }

            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: dropOwner.Value);
            await _eventPipeline.ProcessEventAsync(dropItem);
        }
        else
        {
            foreach (IPlayerEntity s in playerGroup.Members)
            {
                string itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_SHARED, s.Session.UserLanguage, amount, itemName), ChatMessageColorType.Yellow);
            }

            var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: session.PlayerEntity.Id);
            await _eventPipeline.ProcessEventAsync(dropItem);
        }
    }

    private bool ShouldReceiveDrop(IClientSession giftSession, IMonsterEntity monsterEntityToAttack)
    {
        long? tankPlayer = null;
        int highestHits = 0;
        foreach (IBattleEntity damager in monsterEntityToAttack.Damagers)
        {
            if (damager is not IPlayerEntity playerEntity)
            {
                continue;
            }

            if (!playerEntity.HitsByMonsters.TryGetValue(monsterEntityToAttack.Id, out int hits))
            {
                continue;
            }

            if (highestHits > hits)
            {
                continue;
            }

            tankPlayer = playerEntity.Id;
            highestHits = hits;
        }

        if (tankPlayer != null && giftSession.PlayerEntity.Id == tankPlayer.Value)
        {
            return true;
        }

        IPlayerEntity player = giftSession.PlayerEntity;
        if (!monsterEntityToAttack.PlayersDamage.TryGetValue(player.Id, out int damage))
        {
            return false;
        }

        int damageToDealt = (int)(monsterEntityToAttack.MaxHp * 0.05);
        return damageToDealt <= damage;
    }

    private async Task HandleGoldDrops(
        IMonsterEntity monsterEntityToAttack,
        PlayerGroup playerGroup,
        IPlayerEntity firstAttacker,
        bool isByButcherCommand)
    {
        // No dropea oro si el mob no corresponde
        if (monsterEntityToAttack.MonsterRaceType is MonsterRaceType.Fixed or MonsterRaceType.Other)
        {
            return;
        }

        if (monsterEntityToAttack.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        // Oro base desde configuración (YAML + bonuses Premium/Hardcore)
        int gold = GetGold(firstAttacker);

        if (gold <= 0)
        {
            return;
        }

        // Máximo global de oro
        long maxGold = _serverManager.MaxGold;
        if (gold > maxGold)
        {
            gold = (int)maxGold;
        }

        // Penalidad por diferencia de nivel -> afecta probabilidad, no cantidad
        float goldPenalty = CalculateDropPenalty(firstAttacker.Level, monsterEntityToAttack.Level, isGold: true);

        // Chance base de dropear oro
        int randomNumber = 0;
        int rate = _serverManager.GoldDropRate;

        for (int i = 0; i < rate; i++)
        {
            randomNumber += _randomGenerator.RandomNumber();
        }

        // Si falla el chance (considerando penalidad), no suelta oro
        if (randomNumber >= 50 * _serverManager.GoldDropChance * goldPenalty)
        {
            return;
        }

        IClientSession session = firstAttacker.Session;
        if (session?.CurrentMapInstance == null)
        {
            return;
        }

        // ¿BCard que duplica drops?
        int secondChanceDropBCard = session.PlayerEntity.BCardComponent
            .GetAllBCardsInformation(BCardType.DropItemTwice, (byte)AdditionalTypes.DropItemTwice.DoubleDropChance, session.PlayerEntity.Level).firstData;
        bool secondChanceDrop = secondChanceDropBCard != 0 && _randomGenerator.RandomNumber() <= secondChanceDropBCard;

        if (secondChanceDrop)
        {
            session.BroadcastEffectInRange(EffectType.DoubleChanceDrop);
        }

        // Drop directo al inventario (ej. Act4, autoloot, butcher, etc.)
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) ||
            session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED) ||
            monsterEntityToAttack.DropToInventory ||
            session.PlayerEntity.HasAutoLootEnabled ||
            isByButcherCommand)
        {
            var alreadyGifted = new HashSet<long>();

            foreach (IBattleEntity entity in monsterEntityToAttack.Damagers)
            {
                long charId = entity.Id;
                if (!alreadyGifted.Add(charId)) continue;

                IClientSession giftSession = _sessionManager.GetSessionByCharacterId(charId);
                if (giftSession == null ||
                    giftSession.PlayerEntity.MapInstance?.Id != monsterEntityToAttack.MapInstance?.Id ||
                    !ShouldReceiveDrop(giftSession, monsterEntityToAttack))
                {
                    continue;
                }

                await giftSession.EmitEventAsync(new GenerateGoldEvent(gold));
                if (secondChanceDrop)
                {
                    await giftSession.EmitEventAsync(new GenerateGoldEvent(gold));
                }
            }

            return;
        }

        // Drop visible en el mapa
        string itemName = _itemsManager.GetItem((short)ItemVnums.GOLD)?.Name ?? "Gold";

        if (playerGroup == null)
        {
            var dropGold = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, monsterEntityToAttack.Position, (short)ItemVnums.GOLD, gold, ownerId: firstAttacker.Id);
            await _eventPipeline.ProcessEventAsync(dropGold);
            if (secondChanceDrop)
            {
                await _eventPipeline.ProcessEventAsync(dropGold);
            }

            return;
        }

        if (playerGroup.SharingMode == (byte)GroupSharingType.ByOrder)
        {
            long? dropOwner = playerGroup.GetNextOrderedCharacterId(firstAttacker);
            if (!dropOwner.HasValue)
            {
                return;
            }

            foreach (IPlayerEntity s in playerGroup.Members)
            {
                string itemNameTranslated = _gameLanguage.GetLanguage(GameDataType.Item, itemName, s.Session.UserLanguage);
                s.Session.SendChatMessage(
                    s.Session.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_ORDERED, gold, itemNameTranslated,
                        playerGroup.Members.FirstOrDefault(c => c.Id == (long)dropOwner)?.Name),
                    ChatMessageColorType.Yellow);
            }

            var dropGold = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, monsterEntityToAttack.Position, (short)ItemVnums.GOLD, gold, ownerId: dropOwner.Value);
            await _eventPipeline.ProcessEventAsync(dropGold);
            if (secondChanceDrop)
            {
                await _eventPipeline.ProcessEventAsync(dropGold);
            }
        }
        else
        {
            foreach (IPlayerEntity s in playerGroup.Members)
            {
                string itemNameTranslated = _gameLanguage.GetLanguage(GameDataType.Item, itemName, s.Session.UserLanguage);
                s.Session.SendChatMessage(
                    s.Session.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_SHARED, gold, itemNameTranslated),
                    ChatMessageColorType.Yellow);
            }

            var dropGold = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, monsterEntityToAttack.Position, (short)ItemVnums.GOLD, gold, ownerId: session.PlayerEntity.Id);
            await _eventPipeline.ProcessEventAsync(dropGold);
            if (secondChanceDrop)
            {
                await _eventPipeline.ProcessEventAsync(dropGold);
            }
        }
    }

    private int GetGold(IPlayerEntity playerEntity)
    {
        if (!playerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP)
            && playerEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance
            && playerEntity.MapInstance.MapInstanceType != MapInstanceType.PrivateInstance
            && playerEntity.MapInstance.MapInstanceType != MapInstanceType.LandOfDeath
            && playerEntity.MapInstance.MapInstanceType != MapInstanceType.LandOfLife
            && playerEntity.MapInstance.MapInstanceType != MapInstanceType.WorldBossInstance)
        {
            return 0;
        }

        MapGoldConfiguration mapConfig = _generalMapGoldConfiguration.GetGoldConfigurationByMapId(playerEntity.MapInstance.MapVnum);
        if (mapConfig == null)
        {
            return 0;
        }

        int gold = _randomGenerator.RandomNumber(mapConfig.MinRange, mapConfig.MaxRange + 1);

        return gold;
    }
}