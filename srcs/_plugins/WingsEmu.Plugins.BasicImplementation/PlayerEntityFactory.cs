using System;
using System.Collections.Generic;
using System.Linq;
using Mapster;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsAPI.Data.Miniland;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Portals;
using WingsEmu.Game.SnackFood;
using WingsEmu.Game.Warehouse;

namespace NosEmu.Plugins.BasicImplementations;

public class PlayerEntityFactory : IPlayerEntityFactory
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IFamilyManager _familyManager;
    private readonly IFoodSnackComponentFactory _foodSnackComponentFactory;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IMapManager _mapManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IPortalFactory _portalFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;
    private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;
    public PlayerEntityFactory(IFamilyManager familyManager, IRandomGenerator randomGenerator, IMapManager mapManager, ICharacterAlgorithm characterAlgorithm,
        IFoodSnackComponentFactory foodSnackComponentFactory, IAsyncEventPipeline eventPipeline, IBattleEntityAlgorithmService algorithm, IMateEntityFactory mateEntityFactory,
        IPortalFactory portalFactory, IItemsManager itemsManager, IGameItemInstanceFactory itemInstanceFactory, IBuffFactory buffFactory, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
    {
        _familyManager = familyManager;
        _randomGenerator = randomGenerator;
        _mapManager = mapManager;
        _characterAlgorithm = characterAlgorithm;
        _foodSnackComponentFactory = foodSnackComponentFactory;
        _eventPipeline = eventPipeline;
        _algorithm = algorithm;
        _mateEntityFactory = mateEntityFactory;
        _portalFactory = portalFactory;
        _itemsManager = itemsManager;
        _itemInstanceFactory = itemInstanceFactory;
        _buffFactory = buffFactory;
        _characterSizeModifiersConfiguration  = characterSizeModifiersConfiguration;
    }

    public IPlayerEntity CreatePlayerEntity(CharacterDTO characterDto) => new PlayerEntity(characterDto, _familyManager, _randomGenerator, _mapManager, _characterAlgorithm, _foodSnackComponentFactory,
        _eventPipeline, _algorithm, _portalFactory, _itemsManager, _buffFactory, _characterSizeModifiersConfiguration);

    public CharacterDTO CreateCharacterDto(IPlayerEntity playerEntity)
    {
        var characterDto = new CharacterDTO
        {
            Id = playerEntity.Id,
            Hp = playerEntity.Hp,
            Mp = playerEntity.Mp,
            Level = playerEntity.Level,
            Faction = playerEntity.Faction,
            AccountId = playerEntity.AccountId,
            Act4Dead = playerEntity.Act4Dead,
            Act4Kill = playerEntity.Act4Kill,
            Act4Points = playerEntity.Act4Points,
            ArenaWinner = playerEntity.ArenaWinner,
            Biography = playerEntity.Biography,
            BuffBlocked = playerEntity.BuffBlocked,
            Class = playerEntity.Class,
            SubClass = playerEntity.SubClass,
            TierLevel = playerEntity.TierLevel,
            TierExperience = playerEntity.TierExperience,
            Compliment = playerEntity.Compliment,
            Dignity = playerEntity.Dignity,
            EmoticonsBlocked = playerEntity.EmoticonsBlocked,
            ExchangeBlocked = playerEntity.ExchangeBlocked,
            FamilyRequestBlocked = playerEntity.FamilyRequestBlocked,
            FriendRequestBlocked = playerEntity.FriendRequestBlocked,
            Gender = playerEntity.Gender,
            Gold = playerEntity.Gold,
            GroupRequestBlocked = playerEntity.GroupRequestBlocked,
            HairColor = playerEntity.HairColor,
            HairStyle = playerEntity.HairStyle,
            HeroChatBlocked = playerEntity.HeroChatBlocked,
            HeroLevel = playerEntity.HeroLevel,
            HeroXp = playerEntity.HeroXp,
            HpBlocked = playerEntity.HpBlocked,
            IsPetAutoRelive = playerEntity.IsPetAutoRelive,
            IsPartnerAutoRelive = playerEntity.IsPartnerAutoRelive,
            JobLevel = playerEntity.JobLevel,
            JobLevelXp = playerEntity.JobLevelXp,
            LevelXp = playerEntity.LevelXp,
            MapId = playerEntity.MapId,
            MapX = playerEntity.MapX,
            MapY = playerEntity.MapY,
            MasterPoints = playerEntity.MasterPoints,
            MasterTicket = playerEntity.MasterTicket,
            MaxPetCount = playerEntity.MaxPetCount,
            MaxPartnerCount = playerEntity.MaxPartnerCount,
            MinilandInviteBlocked = playerEntity.MinilandInviteBlocked,
            MinilandMessage = playerEntity.MinilandMessage,
            MinilandPoint = playerEntity.MinilandPoint,
            MinilandState = playerEntity.MinilandState,
            MouseAimLock = playerEntity.MouseAimLock,
            Name = playerEntity.Name,
            QuickGetUp = playerEntity.QuickGetUp,
            HideHat = playerEntity.HideHat,
            HideCD = playerEntity.HideCD,
            HideHPMP = playerEntity.HideHPMP,
            UiBlocked = playerEntity.UiBlocked,
            Reput = playerEntity.Reput,
            Slot = playerEntity.Slot,
            SpPointsBonus = playerEntity.SpPointsBonus,
            SpPointsBasic = playerEntity.SpPointsBasic,
            TalentLose = playerEntity.TalentLose,
            TalentSurrender = playerEntity.TalentSurrender,
            TalentWin = playerEntity.TalentWin,
            WhisperBlocked = playerEntity.WhisperBlocked,
            PartnerInventory = playerEntity.PartnerInventory,
            NosMates = playerEntity.NosMates,
            PartnerWarehouse = playerEntity.PartnerWarehouse,
            Bonus = playerEntity.Bonus,
            StaticBuffs = playerEntity.StaticBuffs,
            Quicklist = playerEntity.Quicklist,
            LearnedSkills = playerEntity.LearnedSkills,
            Titles = playerEntity.Titles,
            CompletedScripts = playerEntity.CompletedScripts,
            CompletedQuests = playerEntity.CompletedQuests,
            CompletedPeriodicQuests = playerEntity.CompletedPeriodicQuests,
            ActiveQuests = playerEntity.ActiveQuests,
            MinilandObjects = playerEntity.MinilandObjects,
            Inventory = playerEntity.Inventory,
            EquippedStuffs = playerEntity.EquippedStuffs,
            LifetimeStats = playerEntity.LifetimeStats,
            RespawnType = playerEntity.HomeComponent.RespawnType,
            Act5RespawnType = playerEntity.HomeComponent.Act5RespawnType,
            ReturnPoint = playerEntity.HomeComponent.Return,
            CompletedTimeSpaces = playerEntity.CompletedTimeSpaces,
            RaidRestrictionDto = playerEntity.RaidRestrictionDto,
            RainbowBattleLeaverBusterDto = playerEntity.RainbowBattleLeaverBusterDto,
            IcebreakerLeaverBusterDto = playerEntity.IcebreakerLeaverBusterDto,
            TrophyFragmentsDto = playerEntity.TrophyFragmentsDto,
            BattlepassItemDto = playerEntity.BattlePassItemDto,
            BattlepassQuestDto = playerEntity.BattlePassQuestDto,
            BattlePassOptionDto = playerEntity.BattlePassOptionDto,
            PityDto = playerEntity.PityDto,
            CharacterPrestigeDto = playerEntity.CharacterPrestigeDto,
            LandOfLifeRestrictionDto = playerEntity.LandOfLifeRestrictionDto,
            WorldBossRecordsDto = playerEntity.WorldBossRecordsDto,
            IsAvailableToChangeName = playerEntity.IsAvailableToChangeName,
            FishDto = playerEntity.FishDto,
            FishRewardsEarnedDto = playerEntity.FishRewardsEarnedDto,
            MaxStarTrainerDto = playerEntity.MaxStarTrainerDto,
            TrainerQuestDto = playerEntity.TrainerQuestDto,
            ItemRestrictionDto = playerEntity.ItemRestrictionDto,
            DailyRewardDto = playerEntity.DailyRewardDto,
            CharacterCookingDto = playerEntity.CharacterCookingDto,
            FoodValue = playerEntity.FoodValue
        };

        characterDto.StaticBuffs.RemoveAll(s => s.RemainingTime <= 0);
        characterDto.StaticBuffs = playerEntity.GetSavedBuffs();
        characterDto.Bonus.RemoveAll(s => s.DateEnd.HasValue && DateTime.UtcNow >= s.DateEnd.Value);
        characterDto.Bonus = playerEntity.Bonus;
        characterDto.Quicklist = playerEntity.QuicklistComponent.GetQuicklist();
        characterDto.LearnedSkills = playerEntity.CharacterSkills.Where(x => x.Value != null).Select(s => s.Value.Adapt<CharacterSkillDTO>()).ToList();
        characterDto.ActiveQuests = playerEntity.GetQuestsProgress().Where(x => x != null).Select(s => s.Adapt<CharacterQuestDto>()).ToList();
        characterDto.CompletedScripts = playerEntity.GetCompletedScripts().Where(x => x != null).Select(s => s.Adapt<CompletedScriptsDto>()).ToList();
        characterDto.CompletedQuests = playerEntity.GetCompletedQuests().Where(x => x != null).Select(s => s.Adapt<CharacterQuestDto>()).ToList();
        characterDto.CompletedPeriodicQuests = playerEntity.GetCompletedPeriodicQuests().Where(x => x != null).Select(s => s.Adapt<CharacterQuestDto>()).ToList();
        characterDto.MinilandObjects = playerEntity.Miniland?.MapDesignObjects?.Where(x => x != null).Select(s => s.Adapt<CharacterMinilandObjectDto>()).ToList() ??
            new List<CharacterMinilandObjectDto>();

        List<CharacterInventoryItemDto> notEquippedItems = new();
        List<CharacterInventoryItemDto> equippedItems = new();

        foreach (InventoryItem item in playerEntity.GetAllPlayerInventoryItems())
        {
            if (item == null)
            {
                continue;
            }

            List<CharacterInventoryItemDto> items = item.IsEquipped ? equippedItems : notEquippedItems;

            CharacterInventoryItemDto tmp = item.Adapt<CharacterInventoryItemDto>();
            tmp.ItemInstance = _itemInstanceFactory.CreateDto(item.ItemInstance);

            items.Add(tmp);
        }

        characterDto.Inventory = notEquippedItems;
        characterDto.EquippedStuffs = equippedItems;

        List<PartnerWarehouseItemDto> partnerWarehouse = new();
        foreach (PartnerWarehouseItem item in playerEntity.PartnerWarehouseItems())
        {
            if (item == null)
            {
                continue;
            }

            PartnerWarehouseItemDto tmp = item.Adapt<PartnerWarehouseItemDto>();
            tmp.ItemInstance = _itemInstanceFactory.CreateDto(item.ItemInstance);

            partnerWarehouse.Add(tmp);
        }

        characterDto.PartnerWarehouse = partnerWarehouse;

        characterDto.NosMates = playerEntity.MateComponent.GetMates(s => !s.IsTemporaryPet).Select(mate => _mateEntityFactory.CreateMateDto(mate)).ToList();

        List<CharacterPartnerInventoryItemDto> partnerInventory = new();
        foreach (PartnerInventoryItem item in playerEntity.GetPartnersEquippedItems())
        {
            if (item == null)
            {
                continue;
            }

            CharacterPartnerInventoryItemDto tmp = item.Adapt<CharacterPartnerInventoryItemDto>();
            tmp.ItemInstance = _itemInstanceFactory.CreateDto(item.ItemInstance);

            partnerInventory.Add(tmp);
        }

        characterDto.PartnerInventory = partnerInventory;

        return characterDto;
    }
}