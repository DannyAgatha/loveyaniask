using System;
using System.Collections.Generic;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Relations;
using WingsEmu.Game.Shops;
using WingsEmu.Game.SnackFood;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.Warehouse;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Characters;

public partial class PlayerEntity
{
    private readonly IChargeComponent _chargeComponent;
    private readonly IInventoryComponent _inventory;
    private readonly IAlzanorComponent _alzanorComponent;
    private readonly IRaidComponent _raidComponent;
    private readonly IBubbleComponent _bubbleComponent;
    private int _criticalChance;
    private int _criticalDamage;
    private readonly IEquipmentOptionContainer _eqOptions;
    private readonly IEventTriggerContainer _eventTriggerContainer;
    private readonly IExchangeComponent _exchange;
    private readonly IFoodSnackComponent _foodSnackComponent;
    private readonly IGroupComponent _groupComponent;
    private int _magicDamageMax;
    private int _magicDamageMin;
    private int _magicDefense;
    private int _meleeDamageMax;
    private int _meleeDamageMin;
    private int _meleeDefense;
    private int _meleeDodge;
    private int _meleeHitRate;
    private readonly IPartnerInventoryComponent _partnerInventory;
    private readonly IQuestContainer _questContainer;
    private int _rangedDamageMax;
    private int _rangedDamageMin;
    private int _rangedDefense;
    private int _rangedDodge;
    private int _rangedHitRate;
    private readonly IRelationComponent _relationComponent;
    private byte _speed;

    public void RefreshCharacterStats(bool refreshHpMp = true)
    {
        if (refreshHpMp)
        {
            MaxHp = this.GetMaxHp(_algorithm.GetBasicHpByClass(Class, Level));
            MaxMp = this.GetMaxMp(_algorithm.GetBasicMpByClass(Class, Level));
        }

        _speed = !IsCustomSpeed ? this.GetSpeed(_algorithm.GetSpeed(Class)) : _speed;
        _meleeDamageMin = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_MELEE);
        _meleeDamageMax = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_MELEE);
        _rangedDamageMin = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_RANGED);
        _rangedDamageMax = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_RANGED);
        _magicDamageMin = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_MAGIC);
        _magicDamageMax = _algorithm.GetBaseStatistic(Level, Class, StatisticType.ATTACK_MAGIC);
        _meleeHitRate = _algorithm.GetBaseStatistic(Level, Class, StatisticType.HITRATE_MELEE);
        _rangedHitRate = _algorithm.GetBaseStatistic(Level, Class, StatisticType.HITRATE_RANGED);
        _criticalChance = 0;
        _criticalDamage = 0;
        _meleeDefense = _algorithm.GetBaseStatistic(Level, Class, StatisticType.DEFENSE_MELEE);
        _rangedDefense = _algorithm.GetBaseStatistic(Level, Class, StatisticType.DEFENSE_RANGED);
        _magicDefense = _algorithm.GetBaseStatistic(Level, Class, StatisticType.DEFENSE_MAGIC);
        _meleeDodge = _algorithm.GetBaseStatistic(Level, Class, StatisticType.DODGE_MELEE);
        _rangedDodge = _algorithm.GetBaseStatistic(Level, Class, StatisticType.DODGE_RANGED);
        StatisticsComponent.RefreshPlayerStatistics();
    }

    public ITimeSpaceComponent TimeSpaceComponent { get; }
    public IBCardComponent BCardComponent { get; }
    public IMateComponent MateComponent { get; }
    public IBuffComponent BuffComponent { get; }
    public IShopComponent ShopComponent { get; }
    public IMailNoteComponent MailNoteComponent { get; }

    public bool HasQuestWithQuestType(QuestType questType) => _questContainer.HasQuestWithQuestType(questType);
    public bool HasQuestWithId(int questId) => _questContainer.HasQuestWithId(questId);
    public bool HasCompletedScriptByIndex(int scriptId, int scriptIndex) => _questContainer.HasCompletedScriptByIndex(scriptId, scriptIndex);

    public IEnumerable<CharacterQuest> GetCurrentQuests() => _questContainer.GetCurrentQuests();
    public IEnumerable<CharacterQuest> GetCompletedQuests() => _questContainer.GetCompletedQuests();
    public IEnumerable<CharacterQuestDto> GetCompletedPeriodicQuests() => _questContainer.GetCompletedPeriodicQuests();
    public IEnumerable<CharacterQuest> GetCurrentQuestsByType(QuestType questType) => _questContainer.GetCurrentQuestsByType(questType);
    public IEnumerable<CharacterQuest> GetCurrentQuestsByTypes(IReadOnlyCollection<QuestType> questTypes) => _questContainer.GetCurrentQuestsByTypes(questTypes);
    public IEnumerable<CharacterQuestDto> GetQuestsProgress() => _questContainer.GetQuestsProgress();

    public CharacterQuest GetCurrentQuest(int questId) => _questContainer.GetCurrentQuest(questId);

    public void AddActiveQuest(CharacterQuest quest)
    {
        _questContainer.AddActiveQuest(quest);
    }

    public void RemoveActiveQuest(int questId)
    {
        _questContainer.RemoveActiveQuest(questId);
    }

    public void AddCompletedQuest(CharacterQuest quest)
    {
        _questContainer.AddCompletedQuest(quest);
    }

    public void AddCompletedPeriodicQuest(CharacterQuest quest)
    {
        _questContainer.AddCompletedPeriodicQuest(quest);
    }

    public IEnumerable<CompletedScriptsDto> GetCompletedScripts() => _questContainer.GetCompletedScripts();
    public IEnumerable<CompletedScriptsDto> GetCompletedScriptsByType(TutorialActionType scriptType) => _questContainer.GetCompletedScriptsByType(scriptType);

    public void SaveScript(int scriptId, int scriptIndex, TutorialActionType scriptType, DateTime savingDate) => _questContainer.SaveScript(scriptId, scriptIndex, scriptType, savingDate);

    public CompletedScriptsDto GetLastCompletedScript() => _questContainer.GetLastCompletedScript();
    public CompletedScriptsDto GetLastCompletedScriptByType(TutorialActionType scriptType) => _questContainer.GetLastCompletedScriptByType(scriptType);

    public void IncreasePendingSoundFlowerQuests() => _questContainer.IncreasePendingSoundFlowerQuests();
    public void DecreasePendingSoundFlowerQuests() => _questContainer.DecreasePendingSoundFlowerQuests();
    public int GetPendingSoundFlowerQuests() => _questContainer.GetPendingSoundFlowerQuests();

    public IReadOnlyList<CharacterRelationDTO> GetRelations() => _relationComponent.GetRelations();

    public IEnumerable<CharacterRelationDTO> GetFriendRelations() => _relationComponent.GetFriendRelations();

    public IEnumerable<CharacterRelationDTO> GetBlockedRelations() => _relationComponent.GetBlockedRelations();

    public bool IsBlocking(long targetId) => _relationComponent.IsBlocking(targetId);
    public bool IsFriend(long targetId) => _relationComponent.IsFriend(targetId);
    public bool IsMarried(long targetId) => _relationComponent.IsMarried(targetId);
    public bool IsFriendsListFull() => _relationComponent.IsFriendsListFull();

    public void AddRelation(CharacterRelationDTO relation)
    {
        _relationComponent.AddRelation(relation);
    }

    public void RemoveRelation(long targetCharacterId, CharacterRelationType relationType)
    {
        _relationComponent.RemoveRelation(targetCharacterId, relationType);
    }

    public long GetGroupId() => _groupComponent.GetGroupId();
    public PlayerGroup GetGroup() => _groupComponent.GetGroup();

    public void AddMember(IPlayerEntity member) => _groupComponent.AddMember(member);

    public void RemoveMember(IPlayerEntity member) => _groupComponent.RemoveMember(member);

    public void SetGroup(PlayerGroup playerGroup) => _groupComponent.SetGroup(playerGroup);

    public void RemoveGroup() => _groupComponent.RemoveGroup();

    public bool IsInGroup() => _groupComponent.IsInGroup();

    public bool IsLeaderOfGroup(long characterId) => _groupComponent.IsLeaderOfGroup(characterId);

    public bool IsGroupFull() => _groupComponent.IsGroupFull();


    public IEnumerable<InventoryItem> GetAllPlayerInventoryItems() => _inventory.GetAllPlayerInventoryItems();

    public InventoryItem GetFirstItemByVnum(int vnum) => _inventory.GetFirstItemByVnum(vnum);

    public IEnumerable<InventoryItem> GetItemsByInventoryType(InventoryType type) => _inventory.GetItemsByInventoryType(type);

    public IEnumerable<InventoryItem> EquippedItems => _inventory.EquippedItems;
    public GameItemInstance GetItemInstanceFromEquipmentSlot(EquipmentType type) => _inventory.GetItemInstanceFromEquipmentSlot(type);

    public GameItemInstance MainWeapon => _inventory.MainWeapon;

    public GameItemInstance SecondaryWeapon => _inventory.SecondaryWeapon;

    public GameItemInstance Armor => _inventory.Armor;

    public GameItemInstance Amulet => _inventory.Amulet;

    public GameItemInstance Hat => _inventory.Hat;

    public GameItemInstance Gloves => _inventory.Gloves;

    public GameItemInstance Ring => _inventory.Ring;

    public GameItemInstance Necklace => _inventory.Necklace;

    public GameItemInstance Bracelet => _inventory.Bracelet;

    public GameItemInstance Boots => _inventory.Boots;

    public GameItemInstance Fairy => _inventory.Fairy;

    public GameItemInstance Specialist => _inventory.Specialist;

    public GameItemInstance Mask => _inventory.Mask;

    public GameItemInstance CostumeSuit => _inventory.CostumeSuit;

    public GameItemInstance CostumeHat => _inventory.CostumeHat;

    public GameItemInstance WeaponSkin => _inventory.WeaponSkin;

    public GameItemInstance Wings => _inventory.Wings;

    public GameItemInstance MiniPet => _inventory.MiniPet;

    public bool InventoryIsInitialized => _inventory.InventoryIsInitialized;

    public int CountItemWithVnum(int vnum) => _inventory.CountItemWithVnum(vnum);

    public bool HasSpaceFor(int vnum, int amount = 1) => _inventory.HasSpaceFor(vnum, amount);

    public void AddItemToInventory(InventoryItem inventoryItem)
    {
        _inventory.AddItemToInventory(inventoryItem);
    }

    public bool HasItem(int vnum, int amount = 1) => _inventory.HasItem(vnum, amount);

    public bool RemoveItemFromSlotAndType(short slot, InventoryType type, out InventoryItem removedItem) => _inventory.RemoveItemFromSlotAndType(slot, type, out removedItem);
    public bool RemoveItemAmountByVnum(int vnum, int amount, out InventoryItem removedItem) => _inventory.RemoveItemAmountByVnum(vnum, amount, out removedItem);

    public void EquipItem(InventoryItem item, EquipmentType type, bool force = false) => _inventory.EquipItem(item, type, force);

    public void TakeOffItem(EquipmentType type, short? slot = null, InventoryType? inventoryType = null) => _inventory.TakeOffItem(type, slot, inventoryType);

    public InventoryItem GetInventoryItemFromEquipmentSlot(EquipmentType type) => _inventory.GetInventoryItemFromEquipmentSlot(type);

    public InventoryItem GetItemBySlotAndType(short slot, InventoryType type) => _inventory.GetItemBySlotAndType(slot, type);

    public InventoryItem FindItemWithoutFullStack(int vnum, int amount) => _inventory.FindItemWithoutFullStack(vnum, amount);

    public void AddShells(EquipmentOptionType equipmentOptionType, List<EquipmentOptionDTO> optionDto, bool isMainWeapon)
    {
        _eqOptions.AddShells(equipmentOptionType, optionDto, isMainWeapon);
    }

    public void ClearShells(EquipmentOptionType equipmentOptionType, bool isMainWeapon)
    {
        _eqOptions.ClearShells(equipmentOptionType, isMainWeapon);
    }

    public Dictionary<ShellEffectType, int> GetShellsValues(EquipmentOptionType equipmentOptionType, bool isMainWeapon) => _eqOptions.GetShellsValues(equipmentOptionType, isMainWeapon);

    public int GetMaxWeaponShellValue(ShellEffectType shellEffectType, bool isMainWeapon) => _eqOptions.GetMaxWeaponShellValue(shellEffectType, isMainWeapon);

    public int GetMaxArmorShellValue(ShellEffectType shellEffectType) => _eqOptions.GetMaxArmorShellValue(shellEffectType);

    public void AddCellon(EquipmentType equipmentType, List<EquipmentOptionDTO> optionDto) => _eqOptions.AddCellon(equipmentType, optionDto);

    public void ClearCellon(EquipmentType equipmentType) => _eqOptions.ClearCellon(equipmentType);

    public Dictionary<CellonType, int> GetCellonValues(EquipmentType equipmentType) => _eqOptions.GetCellonValues(equipmentType);

    public int GetCellonValue(EquipmentType equipmentType, CellonType type) => _eqOptions.GetCellonValue(equipmentType, type);

    public void SetExchange(PlayerExchange exchange) => _exchange.SetExchange(exchange);

    public void RemoveExchange() => _exchange.RemoveExchange();

    public PlayerExchange GetExchange() => _exchange.GetExchange();

    public bool IsInExchange() => _exchange.IsInExchange();

    public long GetTargetId() => _exchange.GetTargetId();

    public void SaveBubble(string message) => _bubbleComponent.SaveBubble(message);

    public bool IsUsingBubble() => _bubbleComponent.IsUsingBubble();

    public string GetMessage() => _bubbleComponent.GetMessage();

    public void RemoveBubble() => _bubbleComponent.RemoveBubble();

    public IReadOnlyList<PartnerInventoryItem> PartnerGetEquippedItems(short partnerSlot) => _partnerInventory.PartnerGetEquippedItems(partnerSlot);

    public IReadOnlyList<PartnerInventoryItem> GetPartnersEquippedItems() => _partnerInventory.GetPartnersEquippedItems();

    public void PartnerEquipItem(InventoryItem item, short partnerSlot) => _partnerInventory.PartnerEquipItem(item, partnerSlot);

    public void PartnerEquipItem(GameItemInstance item, short partnerSlot) => _partnerInventory.PartnerEquipItem(item, partnerSlot);

    public void PartnerTakeOffItem(EquipmentType type, short partnerSlot) => _partnerInventory.PartnerTakeOffItem(type, partnerSlot);

    public PartnerInventoryItem PartnerGetEquippedItem(EquipmentType type, short partnerSlot) => _partnerInventory.PartnerGetEquippedItem(type, partnerSlot);

    public void AddPartnerWarehouseItem(GameItemInstance item, short slot) => _partnerInventory.AddPartnerWarehouseItem(item, slot);

    public void RemovePartnerWarehouseItem(short slot) => _partnerInventory.RemovePartnerWarehouseItem(slot);

    public PartnerWarehouseItem GetPartnerWarehouseItem(short slot) => _partnerInventory.GetPartnerWarehouseItem(slot);

    public IReadOnlyList<PartnerWarehouseItem> PartnerWarehouseItems() => _partnerInventory.PartnerWarehouseItems();

    public byte GetPartnerWarehouseSlots() => _partnerInventory.GetPartnerWarehouseSlots();

    public byte GetPartnerWarehouseSlotsWithoutBackpack() => _partnerInventory.GetPartnerWarehouseSlotsWithoutBackpack();

    public bool HasSpaceForPartnerWarehouseItem() => _partnerInventory.HasSpaceForPartnerWarehouseItem();
    public bool HasSpaceForPartnerItemWarehouse(int itemVnum, int amount = 1) => _partnerInventory.HasSpaceForPartnerItemWarehouse(itemVnum, amount);

    public byte RaidDeaths => _raidComponent.RaidDeaths;
    
    public Position PositionBeforeFernonRaidEnter => _raidComponent.PositionBeforeFernonRaidEnter;

    public void SetPositionBeforeFernonRaidEnter(Position position)
    {
        _raidComponent.SetPositionBeforeFernonRaidEnter(position);
    }

    public bool IsInRaidParty => _raidComponent.IsInRaidParty;

    public bool HasRaidStarted => _raidComponent.HasRaidStarted;

    public RaidParty Raid => _raidComponent.Raid;

    public bool IsRaidLeader(long characterId) => _raidComponent.IsRaidLeader(characterId);

    public bool RaidTeamIsFull => _raidComponent.RaidTeamIsFull;

    public void SetRaidParty(RaidParty raidParty)
    {
        _raidComponent.SetRaidParty(raidParty);
    }

    public void AddRaidDeath()
    {
        _raidComponent.AddRaidDeath();
    }

    public void RemoveRaidDeath()
    {
        _raidComponent.RemoveRaidDeath();
    }

    public FoodProgress GetFoodProgress => _foodSnackComponent.GetFoodProgress;
    public SnackProgress GetSnackProgress => _foodSnackComponent.GetSnackProgress;
    public AdditionalFoodProgress GetAdditionalFoodProgress => _foodSnackComponent.GetAdditionalFoodProgress;
    public AdditionalSnackProgress GetAdditionalSnackProgress => _foodSnackComponent.GetAdditionalSnackProgress;

    public bool AddSnack(IGameItem gameItem) => _foodSnackComponent.AddSnack(gameItem);

    public void AddAdditionalSnack(int max, int amount, bool isHp, int cap = 100)
    {
        _foodSnackComponent.AddAdditionalSnack(max, amount, isHp, cap);
    }

    public bool AddFood(IGameItem gameItem) => _foodSnackComponent.AddFood(gameItem);

    public void AddAdditionalFood(int max, int amount, bool isHp, int cap = 100)
    {
        _foodSnackComponent.AddAdditionalFood(max, amount, isHp, cap);
    }

    public void ClearFoodBuffer()
    {
        _foodSnackComponent.ClearFoodBuffer();
    }

    public void ClearSnackBuffer()
    {
        _foodSnackComponent.ClearSnackBuffer();
    }

    public bool IsWarehouseOpen { get; set; }
    public bool IsPartnerWarehouseOpen { get; set; }
}