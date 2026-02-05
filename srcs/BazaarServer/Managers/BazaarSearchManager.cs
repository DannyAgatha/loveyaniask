using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Data.Bazaar;
using WingsAPI.Data.GameData;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsAPI.Packets.Enums.Bazaar.Filter;
using WingsAPI.Packets.Enums.Bazaar.SubFilter;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;

namespace BazaarServer.Managers
{
    public class BazaarSearchManager
    {
        private readonly ILongKeyCachedRepository<ConcurrentDictionary<long, BazaarItemDTO>> _bazaarItemByItemVnum;
        private readonly ILongKeyCachedRepository<(ItemDTO, BazaarLevelFilterType)> _cachedItems;
        private readonly IResourceLoader<ItemDTO> _itemDao;
        private readonly Dictionary<int, NpcMonsterDto> _monsterDtos;

        private readonly Dictionary<BazaarCategoryFilterType, Dictionary<byte, HashSet<int>>> _itemVnumByCategoryAndSubCategory = new();

        private readonly ThreadSafeHashSet<int> _itemVnumsRegistered = new();

        public BazaarSearchManager(ILongKeyCachedRepository<ConcurrentDictionary<long, BazaarItemDTO>> bazaarItemByItemVnum, IResourceLoader<ItemDTO> itemDao,
            ILongKeyCachedRepository<(ItemDTO, BazaarLevelFilterType)> cachedItems, IResourceLoader<NpcMonsterDto> npcMonsterDao)
        {
            _bazaarItemByItemVnum = bazaarItemByItemVnum;
            _itemDao = itemDao;
            _cachedItems = cachedItems;
            _monsterDtos = npcMonsterDao.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult().DistinctBy(x => x.Id).ToDictionary(x => x.Id);
        }

        public async Task Initialize(IEnumerable<BazaarItemDTO> bazaarItemDtos)
        {
            await CategorizeItems();
            await CacheSearchableBazaarItems(bazaarItemDtos);
        }

        private async Task CacheSearchableBazaarItems(IEnumerable<BazaarItemDTO> bazaarItemDtos)
        {
            Log.Info("[BAZAAR_SEARCH_MANAGER] Caching searchable BazaarItems");
            int count = 0;
            foreach (BazaarItemDTO bazaarItemDto in bazaarItemDtos)
            {
                if (bazaarItemDto.GetBazaarItemStatus() != BazaarListedItemType.ForSale)
                {
                    continue;
                }

                AddItem(bazaarItemDto);
                count++;
            }

            Log.Info($"[BAZAAR_SEARCH_MANAGER] Cached: {count} searchable BazaarItems");
        }

        private async Task CategorizeItems()
        {
            Log.Info("[BAZAAR_SEARCH_MANAGER] Categorizing items from DB");
            IEnumerable<ItemDTO> items = await _itemDao.LoadAsync();

            int count = 0;
            var itemsToRegister = new Dictionary<BazaarCategoryFilterType, HashSet<int>>();
            foreach (ItemDTO item in items)
            {
                (BazaarCategoryFilterType category, IEnumerable<byte> subCategories, bool registerForAllSubcategories) = GetCategoryAndSubCategoriesByItemVnum(item);
                if (registerForAllSubcategories && !itemsToRegister.TryAdd(category, new HashSet<int> { item.Id }))
                {
                    itemsToRegister[category].Add(item.Id);
                }

                if (!_itemVnumByCategoryAndSubCategory.ContainsKey(category))
                {
                    _itemVnumByCategoryAndSubCategory.Add(category, new Dictionary<byte, HashSet<int>>());
                }

                foreach (byte subCategory in subCategories)
                {
                    if (!_itemVnumByCategoryAndSubCategory[category].ContainsKey(subCategory))
                    {
                        _itemVnumByCategoryAndSubCategory[category].Add(subCategory, new HashSet<int>());
                    }

                    _itemVnumByCategoryAndSubCategory[category][subCategory].Add(item.Id);
                }


                if (!_itemVnumByCategoryAndSubCategory[category].ContainsKey(0))
                {
                    _itemVnumByCategoryAndSubCategory[category].Add(0, new HashSet<int>());
                }

                _itemVnumByCategoryAndSubCategory[category][0].Add(item.Id);

                _cachedItems.Set(item.Id, (item, GetItemLevelFilterByLevel(item.LevelMinimum, item.IsHeroic)));
                count++;
            }

            foreach (KeyValuePair<BazaarCategoryFilterType, HashSet<int>> itemToRegister in itemsToRegister)
            {
                Type type = GetSubCategoryTypeByCategory(itemToRegister.Key);
                if (type == null)
                {
                    continue;
                }

                foreach (byte value in Enum.GetValues(type))
                {
                    if (!_itemVnumByCategoryAndSubCategory[itemToRegister.Key].ContainsKey(value))
                    {
                        _itemVnumByCategoryAndSubCategory[itemToRegister.Key].Add(value, new HashSet<int>());
                    }

                    foreach (int vnum in itemToRegister.Value)
                    {
                        _itemVnumByCategoryAndSubCategory[itemToRegister.Key][value].Add(vnum);
                    }
                }
            }

            Log.Info($"[BAZAAR_SEARCH_MANAGER] Categorized: {count.ToString()} items");
        }

        private static Type GetSubCategoryTypeByCategory(BazaarCategoryFilterType category)
        {
            switch (category)
            {
                case BazaarCategoryFilterType.Specialist:
                    return typeof(BazaarCategorySpecialistSubFilterType);
                case BazaarCategoryFilterType.Partner:
                    return typeof(BazaarCategoryPartnerSubFilterType);
                case BazaarCategoryFilterType.Pet:
                    return typeof(BazaarCategoryPetSubFilterType);
                case BazaarCategoryFilterType.StoreMount:
                    return typeof(BazaarCategoryConsumerItemSubFilterType);
                default:
                    return null;
            }
        }

        public void AddItem(BazaarItemDTO bazaarItemDto)
        {
            ItemInstanceDTO itemInstanceDto = bazaarItemDto?.ItemInstance;
            if (itemInstanceDto == null)
            {
                return;
            }

            ConcurrentDictionary<long, BazaarItemDTO> dictionary = _bazaarItemByItemVnum.GetOrSet(itemInstanceDto.ItemVNum, () => new ConcurrentDictionary<long, BazaarItemDTO>());
            dictionary[bazaarItemDto.Id] = bazaarItemDto;
            _itemVnumsRegistered.Add(itemInstanceDto.ItemVNum);
        }

        public void RemoveItem(BazaarItemDTO bazaarItemDto)
        {
            if (bazaarItemDto == null)
            {
                return;
            }

            ConcurrentDictionary<long, BazaarItemDTO> dictionary = _bazaarItemByItemVnum.GetOrSet(bazaarItemDto.ItemInstance.ItemVNum, () => new ConcurrentDictionary<long, BazaarItemDTO>());
            dictionary.Remove(bazaarItemDto.Id, out _);
            if (!dictionary.IsEmpty)
            {
                return;
            }

            _bazaarItemByItemVnum.Remove(bazaarItemDto.ItemInstance.ItemVNum);
            _itemVnumsRegistered.Remove(bazaarItemDto.ItemInstance.ItemVNum);
        }

        public IReadOnlyCollection<BazaarItemDTO> SearchBazaarItems(BazaarSearchContext bazaarSearchContext)
        {
            IReadOnlyCollection<int> desiredItemVNums =
                (bazaarSearchContext.ItemVNumFilter ?? GetItemVNumsByCategoryAndSubCategory(bazaarSearchContext.CategoryFilterType, bazaarSearchContext.SubTypeFilter)) ??
                _itemVnumsRegistered.ToArray();

            var itemList = new List<BazaarItemDTO>();
            var tempItemList = new List<BazaarItemDTO>();

            int ignoreCounter = bazaarSearchContext.Index * bazaarSearchContext.AmountOfItemsPerIndex;
            int sendCounter = 0;

            foreach (int itemVnum in desiredItemVNums.OrderBy(x => x))
            {
                ConcurrentDictionary<long, BazaarItemDTO> dictionary = _bazaarItemByItemVnum.Get(itemVnum);
                if (dictionary == null)
                {
                    continue;
                }

                if (ignoreCounter > dictionary.Count && bazaarSearchContext.LevelFilter == BazaarLevelFilterType.All && bazaarSearchContext.RareFilter == BazaarRarityFilterType.All &&
                    bazaarSearchContext.UpgradeFilter == BazaarUpgradeFilterType.All)
                {
                    ignoreCounter -= dictionary.Count;
                    continue;
                }

                (ItemDTO itemDto, BazaarLevelFilterType baseItemLevelFilter) = _cachedItems.Get(itemVnum);

                bool itemLevelIsInstanceDependant = false;
                if (baseItemLevelFilter == BazaarLevelFilterType.All)
                {
                    itemLevelIsInstanceDependant = true;
                }
                else
                {
                    if (bazaarSearchContext.LevelFilter != BazaarLevelFilterType.All && !ItemLevelFilterChecker(bazaarSearchContext.LevelFilter, baseItemLevelFilter))
                    {
                        continue;
                    }
                }

                IOrderedEnumerable<KeyValuePair<long, BazaarItemDTO>> values;
                switch (bazaarSearchContext.OrderFilter)
                {
                    case BazaarSortFilterType.PriceAscending:
                        values = dictionary.OrderBy(x => x.Value.PricePerItem);
                        break;
                    case BazaarSortFilterType.PriceDescending:
                        values = dictionary.OrderByDescending(x => x.Value.PricePerItem);
                        break;
                    case BazaarSortFilterType.AmountAscending:
                        values = dictionary.OrderBy(x => x.Value.Amount - x.Value.SoldAmount);
                        break;
                    case BazaarSortFilterType.AmountDescending:
                        values = dictionary.OrderByDescending(x => x.Value.Amount - x.Value.SoldAmount);
                        break;
                    default:
                        return null;
                }

                foreach ((long _, BazaarItemDTO bazaarItemDto) in values)
                {
                    BazaarListedItemType itemStatus = bazaarItemDto.GetBazaarItemStatus();
                    if (itemStatus != BazaarListedItemType.ForSale)
                    {
                        RemoveItem(bazaarItemDto);
                        continue;
                    }

                    if (bazaarItemDto.ItemInstance.Type == ItemInstanceType.BoxInstance)
                    {
                        BazaarPerfectionFilterType itemRarityFilter = GetPerfectionFilterByInstance(bazaarItemDto.ItemInstance);

                        if ((int)bazaarSearchContext.RareFilter != (int)BazaarPerfectionFilterType.All && (int)itemRarityFilter != (int)bazaarSearchContext.RareFilter)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        BazaarRarityFilterType itemRarityFilter = GetRarityFilterByInstance(bazaarItemDto.ItemInstance);

                        if (bazaarSearchContext.RareFilter != BazaarRarityFilterType.All && itemRarityFilter != bazaarSearchContext.RareFilter)
                        {
                            continue;
                        }
                    }

                    BazaarUpgradeFilterType itemUpgradeFilter = GetUpgradeFilterByInstance(bazaarItemDto.ItemInstance);

                    if (bazaarSearchContext.UpgradeFilter != BazaarUpgradeFilterType.All && itemUpgradeFilter != bazaarSearchContext.UpgradeFilter)
                    {
                        continue;
                    }

                    if (itemLevelIsInstanceDependant)
                    {
                        BazaarLevelFilterType itemLevelFilter = GetItemLevelFilterByInstance(bazaarItemDto.ItemInstance, bazaarItemDto.ItemInstance.Type);

                        if (bazaarSearchContext.LevelFilter != BazaarLevelFilterType.All && itemLevelFilter != bazaarSearchContext.LevelFilter)
                        {
                            continue;
                        }
                    }

                    if (bazaarSearchContext.SubTypeFilter != 0 && bazaarItemDto.ItemInstance.Type == ItemInstanceType.BoxInstance)
                    {
                        if ((UsableItemSubType)itemDto.ItemSubType != UsableItemSubType.RaidBoxOrSealedJajamaruSpOrSealedSakuraBead)
                        {
                            ItemInstanceDTO boxInstanceDto = bazaarItemDto.ItemInstance;

                            if (!GetBoxInstanceSubCategories(boxInstanceDto, itemDto).Contains(bazaarSearchContext.SubTypeFilter))
                            {
                                continue;
                            }
                        }
                    }

                    if (ignoreCounter > 0)
                    {
                        ignoreCounter--;
                        continue;
                    }

                    if (sendCounter >= bazaarSearchContext.AmountOfItemsPerIndex)
                    {
                        break;
                    }

                    sendCounter++;

                    tempItemList.Add(bazaarItemDto);
                }

                itemList.AddRange(tempItemList);
                tempItemList.Clear();

                if (sendCounter >= bazaarSearchContext.AmountOfItemsPerIndex)
                {
                    break;
                }
            }

            return itemList;
        }

        private List<byte> GetBoxInstanceSubCategories(ItemInstanceDTO boxInstanceDto, ItemDTO itemInfo)
        {
            var subCategory = new List<byte>();
            switch ((UsableItemSubType)itemInfo.ItemSubType)
            {
                case UsableItemSubType.PetBead:
                    subCategory.Add(
                        (byte)(boxInstanceDto.HoldingVNum is 0 or null ? BazaarCategoryPetSubFilterType.EmptyPetBead : BazaarCategoryPetSubFilterType.PetBead));
                    break;
                case UsableItemSubType.PartnerBead:
                    if (boxInstanceDto.HoldingVNum is 0 or null)
                    {
                        subCategory.Add((byte)BazaarCategoryPartnerSubFilterType.EmptyPartnerBead);
                        break;
                    }

                    subCategory.Add((byte)BazaarCategoryPartnerSubFilterType.PartnerBead);
                    subCategory.Add((byte)GetPartnerSubCategoryOfAttack(_monsterDtos.GetValueOrDefault(boxInstanceDto.HoldingVNum.Value)));
                    break;
                case UsableItemSubType.PartnerSpHolder:
                    if (boxInstanceDto.HoldingVNum is 0 or null)
                    {
                        subCategory.Add((byte)BazaarCategoryPartnerSubFilterType.EmptyCardHolder);
                        break;
                    }

                    subCategory.Add((byte)GetPartnerSubCategoryOfAttack(_monsterDtos.GetValueOrDefault(boxInstanceDto.HoldingVNum.Value)));
                    break;
                case UsableItemSubType.SpecialistHolder:
                    if (boxInstanceDto.HoldingVNum is 0 or null)
                    {
                        subCategory.Add((byte)BazaarCategorySpecialistSubFilterType.EmptyCardHolder);
                        break;
                    }

                    subCategory.Add(SpMorphToSubCategory(_cachedItems.Get(boxInstanceDto.HoldingVNum.Value).Item1));
                    break;
                case UsableItemSubType.VehicleBead:
                    subCategory.Add((byte)(boxInstanceDto.HoldingVNum is 0 or null ? BazaarCategoryStoreMountSubFilterType.EmptyMountBead : BazaarCategoryStoreMountSubFilterType.MountBead));
                    break;
            }

            return subCategory;
        }

        private (BazaarCategoryFilterType, IEnumerable<byte>, bool) GetCategoryAndSubCategoriesByItemVnum(ItemDTO itemDto)
        {
            BazaarCategoryFilterType category = BazaarCategoryFilterType.Miscellaneous;
            var subCategories = new List<byte>();
            bool registerForAllSubcategories = false;
            switch (itemDto.ItemType)
            {
                case ItemType.Weapon:
                    category = BazaarCategoryFilterType.Weapon;
                    subCategories.Add(ItemTypeClassToSubCategory(itemDto.Class));
                    break;
                case ItemType.Armor:
                    category = BazaarCategoryFilterType.Armour;
                    subCategories.Add(ItemTypeClassToSubCategory(itemDto.Class));
                    break;
                case ItemType.Fashion:
                    category = BazaarCategoryFilterType.Equipment;
                    subCategories.Add(EquipmentTypeToSubCategory1(itemDto.EquipmentSlot));
                    byte? secondSubCategory = EquipmentTypeToSubCategory2(itemDto.EquipmentSlot, itemDto.Sex switch
                    {
                        0 => GenderType.Unisex,
                        1 => GenderType.Male,
                        _ => GenderType.Female
                    });
                    if (secondSubCategory != null)
                    {
                        subCategories.Add(secondSubCategory.Value);
                    }

                    break;
                case ItemType.Jewelry:
                    category = BazaarCategoryFilterType.Accessories;
                    subCategories.Add(EquipmentTypeToSubCategory3(itemDto.EquipmentSlot));
                    break;
                case ItemType.Specialist:
                    category = BazaarCategoryFilterType.Specialist;
                    subCategories.Add(SpMorphToSubCategory(itemDto));
                    break;
                case ItemType.Box:
                    var usableItemSubType = (UsableItemSubType)itemDto.ItemSubType;
                    switch (usableItemSubType)
                    {
                        case UsableItemSubType.PetBead:
                            category = BazaarCategoryFilterType.Pet;
                            registerForAllSubcategories = true;
                            break;
                        case UsableItemSubType.PartnerBead:
                            category = BazaarCategoryFilterType.Partner;
                            registerForAllSubcategories = true;
                            break;
                        case UsableItemSubType.PartnerSpHolder:
                            category = BazaarCategoryFilterType.Partner;
                            registerForAllSubcategories = true;
                            break;
                        case UsableItemSubType.SpecialistHolder:
                            category = BazaarCategoryFilterType.Specialist;
                            registerForAllSubcategories = true;
                            break;
                        case UsableItemSubType.FairyBead:
                            category = BazaarCategoryFilterType.Accessories;
                            subCategories.Add((byte)BazaarCategoryAccessoriesSubFilterType.Fairy);
                            break;
                        case UsableItemSubType.VehicleBead:
                            category = BazaarCategoryFilterType.StoreMount;
                            registerForAllSubcategories = true;
                            break;
                    }

                    break;
                case ItemType.Shell:
                    category = BazaarCategoryFilterType.Shell;
                    var shellItemSubType = (ShellItemSubType)itemDto.ItemSubType;
                    subCategories.Add(shellItemSubType switch
                    {
                        ShellItemSubType.Weapon => (byte)BazaarCategoryShellSubFilterType.Weapon,
                        ShellItemSubType.Armor => (byte)BazaarCategoryShellSubFilterType.Clothing,
                        _ => default
                    });
                    break;
                case ItemType.Main:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.GeneralItems);
                    break;
                case ItemType.Upgrade:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.Material);
                    break;
                case ItemType.Production:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.ProductionItem);
                    break;
                case ItemType.Special:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.SpecialItems);
                    break;
                case ItemType.Potion:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.HealingPotion);
                    break;
                case ItemType.Event:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.Event);
                    break;
                case ItemType.Title:
                    category = BazaarCategoryFilterType.MainItem;
                    subCategories.Add((byte)BazaarCategoryMainItemSubFilterType.Title);
                    break;
                case ItemType.Sell:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.SaleItem);
                    break;
                case ItemType.Food:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.Food);
                    break;
                case ItemType.Snack:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.Snack);
                    break;
                case ItemType.Magical:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.MagicItem);
                    break;
                case ItemType.Material:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.Ingredients);
                    break;
                case ItemType.PetPartnerItem:
                    category = BazaarCategoryFilterType.ConsumerItem;
                    subCategories.Add((byte)BazaarCategoryConsumerItemSubFilterType.PartnerItem);
                    break;
            }

            return (category, subCategories, registerForAllSubcategories);
        }

        private static byte ItemTypeClassToSubCategory(byte itemTypeClass)
        {
            return itemTypeClass switch
            {
                (int)ItemClassType.Adventurer => (byte)BazaarCategoryWeaponArmourSubFilterType.Adventurer,
                (int)ItemClassType.Swordsman => (byte)BazaarCategoryWeaponArmourSubFilterType.Swordsman,
                (int)ItemClassType.Archer => (byte)BazaarCategoryWeaponArmourSubFilterType.Archer,
                (int)ItemClassType.Mage => (byte)BazaarCategoryWeaponArmourSubFilterType.Magician,
                (int)ItemClassType.MartialArtist => (byte)BazaarCategoryWeaponArmourSubFilterType.MartialArtist,
                _ => default
            };
        }

        private static byte EquipmentTypeToSubCategory1(EquipmentType equipmentType)
        {
            return equipmentType switch
            {
                EquipmentType.Hat => (byte)BazaarCategoryEquipmentSubFilterType.Hat,
                EquipmentType.Mask => (byte)BazaarCategoryEquipmentSubFilterType.Accessory,
                EquipmentType.Gloves => (byte)BazaarCategoryEquipmentSubFilterType.Gloves,
                EquipmentType.Boots => (byte)BazaarCategoryEquipmentSubFilterType.Shoes,
                EquipmentType.CostumeSuit => (byte)BazaarCategoryEquipmentSubFilterType.Costume,
                EquipmentType.CostumeHat => (byte)BazaarCategoryEquipmentSubFilterType.CostumeHat,
                EquipmentType.WeaponSkin => (byte)BazaarCategoryEquipmentSubFilterType.CostumeWeapon,
                EquipmentType.Wings => (byte)BazaarCategoryEquipmentSubFilterType.CostumeWings,
                _ => default
            };
        }

        private static byte? EquipmentTypeToSubCategory2(EquipmentType equipmentType, GenderType genderType)
        {
            if (genderType == GenderType.Unisex)
            {
                return null;
            }

            bool isMale = genderType == GenderType.Male;

            return equipmentType switch
            {
                EquipmentType.CostumeSuit => (byte)(isMale ? BazaarCategoryEquipmentSubFilterType.CostumeMale : BazaarCategoryEquipmentSubFilterType.CostumeFemale),
                EquipmentType.CostumeHat => (byte)(isMale ? BazaarCategoryEquipmentSubFilterType.CostumeHatMale : BazaarCategoryEquipmentSubFilterType.CostumeHatFemale),
                _ => null
            };
        }

        private static byte EquipmentTypeToSubCategory3(EquipmentType equipmentType)
        {
            return equipmentType switch
            {
                EquipmentType.Necklace => (byte)BazaarCategoryAccessoriesSubFilterType.Necklace,
                EquipmentType.Ring => (byte)BazaarCategoryAccessoriesSubFilterType.Ring,
                EquipmentType.Bracelet => (byte)BazaarCategoryAccessoriesSubFilterType.Bracelet,
                EquipmentType.Fairy => (byte)BazaarCategoryAccessoriesSubFilterType.Fairy,
                EquipmentType.Amulet => (byte)BazaarCategoryAccessoriesSubFilterType.Amulet,
                _ => default
            };
        }

        private static byte SpMorphToSubCategory(ItemDTO item)
        {
            int morphId = Convert.ToInt32(item.Morph);
            if (!Enum.IsDefined(typeof(MorphType), morphId))
            {
                return default;
            }

            var morph = (MorphType)morphId;
            return morph switch
            {
                MorphType.Pyjama => (byte)BazaarCategorySpecialistSubFilterType.Pyjama,
                MorphType.Warrior => (byte)BazaarCategorySpecialistSubFilterType.Warrior,
                MorphType.Ninja => (byte)BazaarCategorySpecialistSubFilterType.Ninja,
                MorphType.Ranger => (byte)BazaarCategorySpecialistSubFilterType.Ranger,
                MorphType.Assassin => (byte)BazaarCategorySpecialistSubFilterType.Assassin,
                MorphType.RedMagician => (byte)BazaarCategorySpecialistSubFilterType.RedMagician,
                MorphType.HolyMage => (byte)BazaarCategorySpecialistSubFilterType.HolyMage,
                MorphType.Chicken => (byte)BazaarCategorySpecialistSubFilterType.ChickenCostume,
                MorphType.Jajamaru => (byte)BazaarCategorySpecialistSubFilterType.Jajamaru,
                MorphType.Crusader => (byte)BazaarCategorySpecialistSubFilterType.Crusader,
                MorphType.Berserker => (byte)BazaarCategorySpecialistSubFilterType.Berserker,
                MorphType.Destroyer => (byte)BazaarCategorySpecialistSubFilterType.Destroyer,
                MorphType.WildKeeper => (byte)BazaarCategorySpecialistSubFilterType.WildKeeper,
                MorphType.BlueMage => (byte)BazaarCategorySpecialistSubFilterType.BlueMagician,
                MorphType.DarkGunner => (byte)BazaarCategorySpecialistSubFilterType.DarkGunner,
                MorphType.Pirate => (byte)BazaarCategorySpecialistSubFilterType.Pirate,
                MorphType.Gladiator => (byte)BazaarCategorySpecialistSubFilterType.Gladiator,
                MorphType.Cannoneer => (byte)BazaarCategorySpecialistSubFilterType.FireCannoneer,
                MorphType.Volcano => (byte)BazaarCategorySpecialistSubFilterType.Volcano,
                MorphType.BattleMonk => (byte)BazaarCategorySpecialistSubFilterType.BattleMonk,
                MorphType.Scout => (byte)BazaarCategorySpecialistSubFilterType.Scout,
                MorphType.TideLord => (byte)BazaarCategorySpecialistSubFilterType.TideLord,
                MorphType.DeathReaper => (byte)BazaarCategorySpecialistSubFilterType.DeathReaper,
                MorphType.DemonHunter => (byte)BazaarCategorySpecialistSubFilterType.DemonHunter,
                MorphType.Seer => (byte)BazaarCategorySpecialistSubFilterType.Seer,
                MorphType.Renegade => (byte)BazaarCategorySpecialistSubFilterType.Renegade,
                MorphType.AvengingAngel => (byte)BazaarCategorySpecialistSubFilterType.AvengingAngel,
                MorphType.Archmage => (byte)BazaarCategorySpecialistSubFilterType.Archmage,
                MorphType.DraconicFist => (byte)BazaarCategorySpecialistSubFilterType.DraconicFist,
                MorphType.MysticArts => (byte)BazaarCategorySpecialistSubFilterType.MysticArts,
                MorphType.Wedding => (byte)BazaarCategorySpecialistSubFilterType.WeddingCostume,
                MorphType.MasterWolf => (byte)BazaarCategorySpecialistSubFilterType.MasterWolf,
                MorphType.DemonWarrior => (byte)BazaarCategorySpecialistSubFilterType.DemonWarrior,
                _ => default
            };
        }

        private BazaarLevelFilterType GetItemLevelFilterByInstance(ItemInstanceDTO itemInstanceDto, ItemInstanceType instanceType)
        {
            switch (instanceType)
            {
                case ItemInstanceType.BoxInstance:
                case ItemInstanceType.SpecialistInstance:
                    ItemInstanceDTO specialistInstanceDto = itemInstanceDto;
                    return GetItemLevelFilterByLevel(specialistInstanceDto.SpLevel, false);
                default:
                    (ItemDTO itemDto, BazaarLevelFilterType bazaarLevelFilterType) = _cachedItems.Get(itemInstanceDto.ItemVNum);
                    return itemDto.ItemType == ItemType.Shell
                        ? GetItemLevelFilterByLevel(itemInstanceDto.Upgrade, false)
                        : bazaarLevelFilterType;
            }
        }

        private static BazaarLevelFilterType GetItemLevelFilterByLevel(byte level, bool isHeroic)
        {
            if (isHeroic)
            {
                if (level is < 1 or > 60)
                {
                    return BazaarLevelFilterType.ChampionGear;
                }

                return (BazaarLevelFilterType)(Convert.ToByte(Math.Ceiling(level / 10f)) + (byte)BazaarLevelFilterType.ChampionGear);
            }

            if (level is < 1 or > 99)
            {
                return BazaarLevelFilterType.All;
            }

            return (BazaarLevelFilterType)Convert.ToByte(Math.Ceiling(level / 10f));
        }

        private static bool ItemLevelFilterChecker(BazaarLevelFilterType demandedFilterType, BazaarLevelFilterType itemFilterType)
        {
            return demandedFilterType == itemFilterType || demandedFilterType == BazaarLevelFilterType.All || demandedFilterType == BazaarLevelFilterType.ChampionGear && itemFilterType switch
            {
                BazaarLevelFilterType.ChampionLevelOneToTen => true,
                BazaarLevelFilterType.ChampionLevelElevenToTwenty => true,
                BazaarLevelFilterType.ChampionLevelTwentyOneToThirty => true,
                BazaarLevelFilterType.ChampionLevelThirtyOneToForty => true,
                BazaarLevelFilterType.ChampionLevelFortyOneToFifty => true,
                BazaarLevelFilterType.ChampionLevelFiftyOneToSixty => true,
                _ => false
            };
        }

        private static BazaarUpgradeFilterType GetUpgradeFilterByInstance(ItemInstanceDTO itemInstanceDto)
        {
            if (15 < itemInstanceDto.Upgrade)
            {
                return BazaarUpgradeFilterType.All;
            }

            return (BazaarUpgradeFilterType)(itemInstanceDto.Upgrade + 1);
        }

        private static BazaarRarityFilterType GetRarityFilterByInstance(ItemInstanceDTO itemInstanceDto)
        {
            if (itemInstanceDto.Rarity is < 0 or > 8)
            {
                return BazaarRarityFilterType.All;
            }

            return (BazaarRarityFilterType)(itemInstanceDto.Rarity + 1);
        }

        private static BazaarPerfectionFilterType GetPerfectionFilterByInstance(ItemInstanceDTO itemInstanceDto)
        {
            const int max = 100;
            return itemInstanceDto.SpStoneUpgrade switch
            {
                < 1 or < max => BazaarPerfectionFilterType.All,
                max => BazaarPerfectionFilterType.NintyOneToHundred,
                _ => (BazaarPerfectionFilterType)Convert.ToByte(Math.Ceiling(itemInstanceDto.SpStoneUpgrade / 10f))
            };
        }

        private static BazaarCategoryPartnerSubFilterType GetPartnerSubCategoryOfAttack(NpcMonsterDto npcMonster)
        {
            return npcMonster.AttackType switch
            {
                AttackType.Melee => BazaarCategoryPartnerSubFilterType.CloseAttack,
                AttackType.Ranged => BazaarCategoryPartnerSubFilterType.RemoteAttack,
                AttackType.Magical => BazaarCategoryPartnerSubFilterType.Magic,
                _ => BazaarCategoryPartnerSubFilterType.All
            };
        }

        private IReadOnlyCollection<int> GetItemVNumsByCategoryAndSubCategory(BazaarCategoryFilterType categoryFilterType, byte subTypeFilter)
        {
            if (categoryFilterType == BazaarCategoryFilterType.All || !_itemVnumByCategoryAndSubCategory.ContainsKey(categoryFilterType) ||
                !_itemVnumByCategoryAndSubCategory[categoryFilterType].ContainsKey(subTypeFilter))
            {
                return null;
            }

            return _itemVnumByCategoryAndSubCategory[categoryFilterType][subTypeFilter];
        }
    }
}