using System.Collections.Generic;
using System.Linq;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Maps;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Monsters;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Portals;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;
using WingsAPI.Data.Drops;
using WingsAPI.Data.Shops;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Recipes;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.DTOs.Shops;
using WingsEmu.Game._enum;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

public static class ImportObjectExtensions
{
    public const bool IS_SEALED_EVENT_ACTIVE = true;

    public static List<DropDTO> ToDto(this DropObject obj)
    {
        var drops = new List<DropDTO>();

        if (obj.MonsterVnums != null)
        {
            drops.AddRange(obj.MonsterVnums.Select(vnum => new DropDTO
            {
                Amount = obj.Quantity,
                DropChance = obj.Chance,
                ItemVNum = obj.ItemVNum,
                MonsterVNum = vnum
            }));
        }

        if (obj.MapIds != null)
        {
            drops.AddRange(obj.MapIds.Select(mapId => new DropDTO
            {
                Amount = obj.Quantity,
                DropChance = obj.Chance,
                ItemVNum = obj.ItemVNum,
                MapId = mapId
            }));
        }

        if (obj.Races != null)
        {
            drops.AddRange(obj.Races.Select(raceDrop => new DropDTO
            {
                Amount = obj.Quantity,
                DropChance = obj.Chance,
                ItemVNum = obj.ItemVNum,
                RaceType = raceDrop[0],
                RaceSubType = raceDrop[1]
            }));
        }

        if (obj.MonsterVnums?.Any() != false || obj.MapIds?.Any() != false || obj.Races?.Any() != false)
        {
            return drops;
        }

        drops.Add(new DropDTO
        {
            Amount = obj.Quantity,
            DropChance = obj.Chance,
            ItemVNum = obj.ItemVNum
        });

        return drops;
    }

    public static PortalDTO ToDto(this PortalObject portal) =>
        new PortalDTO
        {
            Type = portal.Type,
            DestinationMapId = portal.DestinationMapId,
            DestinationX = portal.DestinationX,
            DestinationY = portal.DestinationY,
            IsDisabled = portal.IsDisabled,
            SourceX = portal.SourceX,
            SourceY = portal.SourceY,
            SourceMapId = portal.SourceMapId,
            RaidType = portal.RaidType,
            MapNameId = portal.MapNameId,
            LevelRequired = portal.LevelRequired,
            HeroLevelRequired = portal.HeroLevelRequired
        };

    public static ServerMapDto ToDto(this ConfiguredMapObject toDto) =>
        new ServerMapDto
        {
            Id = toDto.MapId,
            AmbientId = toDto.AmbientId,
            MapVnum = toDto.MapVnum,
            MusicId = toDto.MusicId,
            NameId = toDto.NameId,
            Flags = toDto.Flags ?? new List<MapFlags>()
        };

    public static ItemBoxDto ToDtos(this RandomBoxObject obj)
    {
        if (obj.Categories.Count < 0 ||
            (obj.ItemVnum == (short)ItemVnums.SEALED_TREASURE_CHEST && obj.SealedEvent.Count < 0))
        {
            return null;
        }

        var list = obj.Categories
            .Where(category => category.Items != null && category.Items.Count > 0)
            .SelectMany(category => category.Items.Select(item => new ItemBoxItemDto
            {
                ItemGeneratedAmount = (short)item.Quantity,
                ItemGeneratedUpgrade = item.Upgrade,
                ItemGeneratedVNum = (short)item.ItemVnum,
                ItemGeneratedRandomRarity = item.RandomRarity,
                MaximumOriginalItemRare = item.MaximumRandomRarity,
                MinimumOriginalItemRare = item.MinimumRandomRarity,
                Probability = (short)category.Chance,
                ItemGeneratedRarity = item.Rarity
            }))
            .ToList();

        if (IS_SEALED_EVENT_ACTIVE && obj.ItemVnum == (short)ItemVnums.SEALED_TREASURE_CHEST)
        {
            var sealedEventItems = obj.SealedEvent
                .Where(sealedEvent => sealedEvent.Items != null && sealedEvent.Items.Count != 0)
                .SelectMany(sealedEvent => sealedEvent.Items.Select(item => new ItemBoxItemDto
                {
                    ItemGeneratedAmount = (short)item.Quantity,
                    ItemGeneratedUpgrade = item.Upgrade,
                    ItemGeneratedVNum = (short)item.ItemVnum,
                    ItemGeneratedRandomRarity = item.RandomRarity,
                    MaximumOriginalItemRare = item.MaximumRandomRarity,
                    MinimumOriginalItemRare = item.MinimumRandomRarity,
                    Probability = (short)sealedEvent.Chance,
                    ItemGeneratedRarity = item.Rarity
                }))
                .ToList();

            list.AddRange(sealedEventItems);
        }

        return new ItemBoxDto
        {
            Id = obj.ItemVnum,
            MinimumRewards = obj.MinimumRewards,
            MaximumRewards = obj.MaximumRewards,
            ItemBoxType = ItemBoxType.RANDOM_PICK,
            ShowsRaidBoxPanelOnOpen = obj.HideRewardInfo == false,
            Items = list
        };
    }

    public static ItemBoxDto ToDto(this ItemBoxImportFile obj)
    {
        if (obj.Items.Count == 0)
        {
            return null;
        }

        var list = obj.Items.Select(item => new ItemBoxItemDto
            {
                ItemGeneratedAmount = (short)item.Quantity,
                ItemGeneratedUpgrade = item.Upgrade,
                ItemGeneratedVNum = (short)item.ItemVnum,
                ItemGeneratedRarity = item.Rarity,
                ItemGeneratedRandomRarity = item.RandomRarity,
                MaximumOriginalItemRare = item.MaximumRandomRarity,
                MinimumOriginalItemRare = item.MinimumRandomRarity
            })
            .ToList();

        return new ItemBoxDto
        {
            Id = obj.ItemVnum,
            MinimumRewards = obj.MinimumRewards,
            MaximumRewards = obj.MaximumRewards,
            ItemBoxType = ItemBoxType.BUNDLE,
            ShowsRaidBoxPanelOnOpen = obj.ShowRaidBoxModalOnOpen,
            Items = list
        };
    }

    public static ShopSkillDTO ToDto(this MapNpcShopSkillObject obj, byte tabId, short position) => new()
    {
        SkillVNum = obj.SkillVnum,
        Type = tabId,
        Slot = position
    };


    public static RecipeDTO ToDto(this RecipeObject obj) => new()
    {
        ProducedItemVnum = obj.ItemVnum,
        Amount = obj.Quantity,
        ProducerNpcVnum = obj.ProducerNpcVnum,
        ProducerMapNpcId = obj.ProducerMapNpcId,
        ProducerItemVnum = obj.ProducerItemVnum,
        ProducerSkillVnum = obj.ProducerSkillVnum,
        ProducedChefXp = obj.ProducedChefXp,
        LimitCrafting = obj.LimitCrafting,
        BearingChef = obj.BearingChef
    };

    public static RecipeItemDTO ToDto(this RecipeItemObject obj, short slot) => new()
    {
        ItemVNum = obj.ItemVnum,
        Slot = slot,
        Amount = obj.Quantity
    };

    public static ShopItemDTO ToDto(this MapNpcShopItemObject obj, byte shopType, short position) =>
        new()
        {
            Type = shopType,
            ItemVNum = (short)obj.ItemVnum,
            Rare = obj.Rarity,
            Slot = position,
            Upgrade = obj.Upgrade,
            Price = obj.Price,
            Color = obj.Design
        };

    public static ShopDTO ToDto<T>(this MapNpcShopObject<T> shop) =>
        new()
        {
            Name = shop.Name,
            MenuType = shop.MenuType,
            ShopType = shop.ShopType
        };

    public static MapNpcDTO ToDto(this MapNpcObject obj) =>
        new()
        {
            Id = obj.MapNpcId,
            NpcVNum = (short)obj.NpcMonsterVnum,
            MapX = obj.PosX,
            MapY = obj.PosY,
            MapId = obj.MapId,
            Dialog = (short)obj.DialogId,
            QuestDialog = obj.QuestDialog,
            Effect = (short)obj.Effect,
            EffectDelay = (short)obj.EffectDelay,
            Direction = obj.Direction,
            IsMoving = obj.IsMoving,
            IsSitting = obj.IsSitting,
            IsDisabled = obj.IsDisabled,
            CanAttack = obj.CanAttack,
            HasGodMode = obj.HasGodMode,
            CustomName = obj.CustomName,
            Faction = obj.Faction
        };


    public static TeleporterDTO ToDto(this TeleporterObject teleporterObject) => new()
    {
        Index = teleporterObject.Index,
        Type = teleporterObject.Type,
        MapId = teleporterObject.MapId,
        MapNpcId = teleporterObject.MapNpcId,
        MapX = teleporterObject.MapX,
        MapY = teleporterObject.MapY
    };

    public static MapMonsterDTO ToDto(this MapMonsterObject obj) => new()
    {
        MapId = obj.MapId,
        MonsterVNum = obj.MonsterVNum,
        Direction = obj.Position,
        MapX = obj.MapX,
        MapY = obj.MapY,
        IsMoving = obj.IsMoving
    };
}