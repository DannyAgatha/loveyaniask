using System;
using System.Collections.Generic;
using System.Linq;
using Mapster;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class GameItemInstanceFactory : IGameItemInstanceFactory
{
    private readonly IItemsManager _itemsManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IDropRarityConfigurationProvider _rarityConfigurationProvider;
    private readonly IShellGenerationAlgorithm _shellGenerationAlgorithm;
    private readonly GeneralServerConfiguration _generalServerConfiguration;
    
    public GameItemInstanceFactory(IItemsManager itemsManager, IRandomGenerator randomGenerator, IDropRarityConfigurationProvider rarityConfigurationProvider,
        IShellGenerationAlgorithm shellGenerationAlgorithm, GeneralServerConfiguration generalServerConfiguration)
    {
        _itemsManager = itemsManager;
        _randomGenerator = randomGenerator;
        _rarityConfigurationProvider = rarityConfigurationProvider;
        _shellGenerationAlgorithm = shellGenerationAlgorithm;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public GameItemInstance CreateItem(ItemInstanceDTO dto)
    {
        GameItemInstance instance = dto.Adapt<GameItemInstance>();
        if (instance.SerialTracker == null)
        {
            instance.SerialTracker = Guid.NewGuid();
        }

        return instance;
    }

    public ItemInstanceDTO CreateDto(GameItemInstance instance)
    {
        ItemInstanceDTO dto = instance.Adapt<ItemInstanceDTO>();
        if (dto.SerialTracker == null)
        {
            dto.SerialTracker = Guid.NewGuid();
        }

        return dto;
    }

    public GameItemInstance CreateItem(int itemVnum) => CreateItem(itemVnum, 1, 0, 0, 0);
    public GameItemInstance CreateItem(int itemVnum, bool isMateLimited) => CreateItem(itemVnum, 1, 0, 0, 0, isMateLimited);

    public GameItemInstance CreateItem(int itemVnum, int amount) => CreateItem(itemVnum, amount, 0, 0, 0);

    public GameItemInstance CreateItem(int itemVnum, int amount, byte upgrade) => CreateItem(itemVnum, amount, upgrade, 0, 0);
    public GameItemInstance CreateItem(int itemVnum, int amount, byte upgrade, sbyte rare) => CreateItem(itemVnum, amount, upgrade, rare, 0);

    public GameItemInstance CreateItem(int itemVnum, int amount, byte upgrade, sbyte rare, byte design, bool isMateLimited = false)
    {
        IGameItem newGameItem = _itemsManager.GetItem(itemVnum);
        if (newGameItem == null)
        {
            return null;
        }

        bool isNotStackable = newGameItem.IsNotStackableInventoryType();

        if (amount > _generalServerConfiguration.MaxItemAmount && itemVnum != (int)ItemVnums.GOLD)
        {
            amount = _generalServerConfiguration.MaxItemAmount;
        }

        if (isNotStackable && amount != 1)
        {
            amount = 1;
        }

        switch (newGameItem.ItemType)
        {
            case ItemType.Shell:
                return new GameItemInstance
                {
                    Type = ItemInstanceType.WearableInstance,
                    ItemVNum = itemVnum,
                    Amount = amount,
                    Upgrade = upgrade == 0 ? (byte)_randomGenerator.RandomNumber(newGameItem.ShellMinimumLevel, newGameItem.ShellMaximumLevel) : upgrade,
                    Rarity = rare == 0 ? _rarityConfigurationProvider.GetRandomRarity(ItemType.Shell) : rare,
                    Design = design,
                    DurabilityPoint = newGameItem.LeftUsages,
                    EquipmentOptions = []
                };
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Fashion:
            case ItemType.Jewelry:
                var item = new GameItemInstance
                {
                    Type = ItemInstanceType.WearableInstance,
                    ItemVNum = itemVnum,
                    Amount = amount,
                    Upgrade = upgrade,
                    Rarity = rare,
                    Design = design,
                    DurabilityPoint = newGameItem.LeftUsages,
                    EquipmentOptions = [],
                    CarvedRunes = new CarvedRunesDto()
                };

                if (item.Rarity != 0)
                {
                    item.SetRarityPoint(_randomGenerator);
                }

                if (item.GameItem.IsHeroic && item.Rarity != 0)
                {
                    ShellType shellType = item.GameItem.ItemType == ItemType.Armor ? ShellType.PvpShellArmor : ShellType.PvpShellWeapon;
                    IEnumerable<EquipmentOptionDTO> shellOptions = _shellGenerationAlgorithm.GenerateShell((byte)shellType, item.Rarity == 8 ? 7 : item.Rarity, 99).ToList();
                    item.EquipmentOptions ??= [];
                    item.EquipmentOptions.AddRange(shellOptions);
                }

                if (item.GameItem.EquipmentSlot == EquipmentType.Gloves || item.GameItem.EquipmentSlot == EquipmentType.Boots)
                {
                    item.FireResistance = (short)(item.GameItem.FireResistance * upgrade);
                    item.WaterResistance = (short)(item.GameItem.WaterResistance * upgrade);
                    item.LightResistance = (short)(item.GameItem.LightResistance * upgrade);
                    item.DarkResistance = (short)(item.GameItem.DarkResistance * upgrade);
                }
                return item;
            case ItemType.Specialist:
                if (newGameItem.IsPartnerSpecialist)
                {
                    return new GameItemInstance
                    {
                        Type = ItemInstanceType.SpecialistInstance,
                        ItemVNum = itemVnum,
                        Amount = amount,
                        Agility = 0,
                        PartnerSkills = [],
                    };
                }

                return new GameItemInstance
                {
                    Type = ItemInstanceType.SpecialistInstance,
                    ItemVNum = itemVnum,
                    Amount = amount,
                    SpLevel = 1,
                    Upgrade = upgrade
                };
            case ItemType.Box:
                byte level = newGameItem.ItemSubType switch
                {
                    0 => (byte)newGameItem.Data[2],
                    1 => (byte)newGameItem.Data[2],
                    _ => 1
                };

                return new GameItemInstance
                {
                    Type = ItemInstanceType.BoxInstance,
                    ItemVNum = itemVnum,
                    HoldingVNum = newGameItem.Data[1],
                    Amount = amount,
                    Rarity = rare,
                    Design = design,
                    SpLevel = level,
                    IsLimitedMatePearl = isMateLimited
                };
        }

        return new GameItemInstance(itemVnum, amount, upgrade, rare, design);
    }

    public GameItemInstance CreateSpecialistCard(int itemVnum, byte spLevel = 1, byte upgrade = 0, byte spStoneUpgrade = 0,
        byte spDamage = 0, byte spDefence = 0, byte spElement = 0, byte spHp = 0,
        byte spFire = 0, byte spWater = 0, byte spLight = 0, byte spDark = 0)
    {
        IGameItem newGameItem = _itemsManager.GetItem(itemVnum);
        if (newGameItem == null)
        {
            return null;
        }

        return new GameItemInstance
        {
            Type = ItemInstanceType.SpecialistInstance,
            ItemVNum = itemVnum,
            Amount = 1,
            SpLevel = spLevel,
            Upgrade = upgrade,
            SpStoneUpgrade = spStoneUpgrade,
            SpDamage = spDamage,
            SpDefence = spDefence,
            SpElement = spElement,
            SpHP = spHp,
            SpFire = spFire,
            SpWater = spWater,
            SpLight = spLight,
            SpDark = spDark,
        };
    }

    public GameItemInstance DuplicateItem(GameItemInstance gameInstance) => gameInstance.Adapt<GameItemInstance>();
}