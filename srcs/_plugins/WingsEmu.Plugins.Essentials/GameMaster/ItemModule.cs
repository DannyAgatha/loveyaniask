using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Scheduler;
using Qmmands;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Super Game Master")]
[Description("Module related to items Super Game Master commands.")]
[RequireAuthority(AuthorityType.SGM)]
public class ItemModule : SaltyModuleBase
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _language;
    private readonly IPortalFactory _portalFactory;
    private readonly IScheduler _scheduler;

    public ItemModule(IGameLanguageService language, IGameItemInstanceFactory gameItemInstanceFactory, IScheduler scheduler, IPortalFactory portalFactory)
    {
        _language = language;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _scheduler = scheduler;
        _portalFactory = portalFactory;
    }

    [Command("cportal")]
    [Description("Creates a temporary portal to the place you specify.")]
    public async Task<SaltyCommandResult> CreateAPortal(
        [Description("The Id of the Destination Map.")]
        short mapDestId,
        [Description("The value of the coordinate X where the portal will deploy you.")]
        short mapDestX,
        [Description("The value of the coordinate Y where the portal will deploy you.")]
        short mapDestY,
        [Description("The Portal Duration In Seconds")]
        int timeInSeconds = 0)
    {
        IPortalEntity portal = _portalFactory.CreatePortal(PortalType.TSNormal, Context.Player.CurrentMapInstance, Context.Player.PlayerEntity.Position, mapDestId, new Position(mapDestX, mapDestY));
        Context.Player.CurrentMapInstance.AddPortalToMap(portal, _scheduler, timeInSeconds, timeInSeconds > 0);
        return new SaltyCommandResult(true);
    }

    [Command("rportal", "remove-portal")]
    [Description("Removes the closest temporary portal.")]
    public async Task<SaltyCommandResult> RemovePortal()
    {
        IPortalEntity portalToDelete = Context.Player.CurrentMapInstance.GetClosestPortal(Context.Player.PlayerEntity.PositionX, Context.Player.PlayerEntity.PositionY);

        if (portalToDelete == null)
        {
            return new SaltyCommandResult(false, "There are no temporary portals in your area.");
        }

        if (!Context.Player.PlayerEntity.Position.IsInRange(new Position(portalToDelete.PositionX, portalToDelete.PositionY), 3))
        {
            return new SaltyCommandResult(false, $"You're not close enough to the temporary portal. (X: {portalToDelete.PositionX}; Y: {portalToDelete.PositionY})");
        }

        Context.Player.CurrentMapInstance.DeletePortal(portalToDelete);
        return new SaltyCommandResult(true, "The temporary portal has been removed successfully!");
    }

    [Command("pearl")]
    [Description("Creates a pearl with the vnum of the mate you desire.")]
    public async Task<SaltyCommandResult> CreateMatePearl(
        [Description("The VNum of the mate you want.")]
        int item, bool isLimited)
    {
        IClientSession session = Context.Player;

        GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem(item, isLimited);

        await session.AddNewItemToInventory(itemInstance);
        return new SaltyCommandResult(true);
    }

    [Command("item")]
    [Description("Create an Item")]
    public async Task<SaltyCommandResult> CreateitemAsync(
        [Description("VNUM Item.")] short itemvnum)
    {
        IClientSession session = Context.Player;
        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(itemvnum);
        await session.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, $"Created item: {_language.GetLanguage(GameDataType.Item, newItem.GameItem.Name, Context.Player.UserLanguage)}");
    }

    [Command("fairy-level", "flevel", "flvl", "fairylevel")]
    [Description("Set equipped fairy level.")]
    public async Task<SaltyCommandResult> FairyLevel([Description("Fairy level.")] short fairyLevel)
    {
        IClientSession session = Context.Player;
        if (session.PlayerEntity.Fairy == null || session.PlayerEntity.Fairy.IsEmpty)
        {
            return new SaltyCommandResult(false, "No fairy equipped.");
        }

        session.PlayerEntity.Fairy.ElementRate = fairyLevel;
        session.RefreshFairy();

        return new SaltyCommandResult(true, $"Your fairy level has been set to {fairyLevel}%!");
    }

    [Command("sp")]
    [Description("Create a Specialist Card with upgrade.")]
    public async Task<SaltyCommandResult> SpAsync(
        [Description("SP VNUM.")] short spvnum,
        [Description("Upgrade.")] byte upgrade = 0)
    {
        IClientSession session = Context.Player;

        GameItemInstance newItem = _gameItemInstanceFactory.CreateSpecialistCard(spvnum, upgrade: upgrade);
        await session.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, $"Specialist Card: {_language.GetLanguage(GameDataType.Item, newItem.GameItem.Name, session.UserLanguage)} created.");
    }

    [Command("itempack")]
    [Description("Create a kit of items")]
    public async Task<SaltyCommandResult> CreateitemAsync(
    [Description("adventurer = 0, swordsman = 1, archer = 2, mage = 3, martialartist = 4, " +
                "resistance = 5, jewellery = 6, wings = 7, fairy = 8, consumables = 9, costumes = 10, mounts = 11, upgrade = 12")] int kitType)
    {
        IClientSession session = Context.Player;

        if (kitType < 0 || kitType > 12)
        {
            return new SaltyCommandResult(false, "adventurer = 0, swordsman = 1, archer = 2, mage = 3, martialartist = 4, " +
                "resistance = 5, jewellery = 6, wings = 7, fairy = 8, consumables = 9, costumes = 10, mounts = 11, upgrade = 12");
        }

        switch (kitType)
        {
            case 0:
                GameItemInstance[] adventurerItems = 
                {
                    _gameItemInstanceFactory.CreateItem(7, 1, 10, 7), // Adventurer's Sword
                    _gameItemInstanceFactory.CreateItem(11, 1, 10, 7), // Adventurer's Catapult
                    _gameItemInstanceFactory.CreateItem(17, 1, 10, 7), // Warm Cloak Set
                    _gameItemInstanceFactory.CreateItem(900, 1, 20), // Pyjama Specialist Card
                    _gameItemInstanceFactory.CreateItem(907, 1, 20), // Chicken Specialist Card
                    _gameItemInstanceFactory.CreateItem(908, 1, 20), // Jajamaru Specialist Card
                    _gameItemInstanceFactory.CreateItem(4099, 1, 20), // Pirate Specialist Card
                    _gameItemInstanceFactory.CreateItem(4416, 1, 20), // Wedding Costume Specialist Card
                    _gameItemInstanceFactory.CreateItem(4562, 1, 20), // Angler Specialist Card
                    _gameItemInstanceFactory.CreateItem(4575, 1, 20) // Chef Specialist Card
                };
                
                foreach (GameItemInstance item in adventurerItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 1:
                GameItemInstance[] swordmanItems = 
                {
                    _gameItemInstanceFactory.CreateItem(4618, 1, 10, 8), // Dragonslayer
                    _gameItemInstanceFactory.CreateItem(4626, 1, 10, 8),// Dragon Crystal Crossbow
                    _gameItemInstanceFactory.CreateItem(4634, 1, 10, 8), // Dragonslayer Armour
                    _gameItemInstanceFactory.CreateItem(901, 1, 20), // Warrior Specialist Card
                    _gameItemInstanceFactory.CreateItem(902, 1, 20), // Ninja Specialist Card
                    _gameItemInstanceFactory.CreateItem(909, 1, 20), // Crusader Specialist Card
                    _gameItemInstanceFactory.CreateItem(910, 1, 20), // Berserker Specialist Card
                    _gameItemInstanceFactory.CreateItem(4500, 1, 20), // Gladiator Specialist Card
                    _gameItemInstanceFactory.CreateItem(4497, 1, 20), // Battle Monk Specialist Card
                    _gameItemInstanceFactory.CreateItem(4493, 1, 20), // Death Reaper Specialist Card
                    _gameItemInstanceFactory.CreateItem(4489, 1, 20), // Renegade Specialist Card
                    _gameItemInstanceFactory.CreateItem(4581, 1, 20), // Waterfall Berserker Specialist Card
                    _gameItemInstanceFactory.CreateItem(8521, 1, 20), // Dragon Knight Specialist Card
                    _gameItemInstanceFactory.CreateItem(8712, 1, 20), // Stone Breaker Specialist Card

                };
                
                foreach (GameItemInstance item in swordmanItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 2:
                GameItemInstance[] archerItems = 
                {
                    _gameItemInstanceFactory.CreateItem(4620, 1, 10, 8), // Breath of Destruction
                    _gameItemInstanceFactory.CreateItem(4628, 1, 10, 8), // Dragon Bone Dagger
                    _gameItemInstanceFactory.CreateItem(4636, 1, 10, 8), // Dragon Hunter Uniform
                    _gameItemInstanceFactory.CreateItem(903, 1, 20), // Ranger Specialist Card
                    _gameItemInstanceFactory.CreateItem(904, 1, 20), // Assassin Specialist Card
                    _gameItemInstanceFactory.CreateItem(911, 1, 20), // Destroyer Specialist Card
                    _gameItemInstanceFactory.CreateItem(912, 1, 20), // Wild Keeper Specialist Card
                    _gameItemInstanceFactory.CreateItem(4501, 1, 20), // Fire Cannoneer Specialist Card
                    _gameItemInstanceFactory.CreateItem(4498, 1, 20), // Scout Specialist Card
                    _gameItemInstanceFactory.CreateItem(4492, 1, 20), // Demon Hunter Specialist Card
                    _gameItemInstanceFactory.CreateItem(4488, 1, 20), // Avenging Angel Specialist Card
                    _gameItemInstanceFactory.CreateItem(4582, 1, 20), // Sunchaser Specialist Card
                    _gameItemInstanceFactory.CreateItem(8522, 1, 20), // Blaster Specialist Card
                    _gameItemInstanceFactory.CreateItem(8713, 1, 20), // Fog Hunter Specialist Card
                };
                
                foreach (GameItemInstance item in archerItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 3:
                GameItemInstance[] mageItems = 
                {
                    _gameItemInstanceFactory.CreateItem(4622, 1, 10, 8), // Dragon Soul Wand
                    _gameItemInstanceFactory.CreateItem(4630, 1, 10, 8), // Freeze Spell Gun
                    _gameItemInstanceFactory.CreateItem(4638, 1, 10, 8), // Frost Scale Robe
                    _gameItemInstanceFactory.CreateItem(905, 1, 20), // Red Magician Specialist Card
                    _gameItemInstanceFactory.CreateItem(906, 1, 20), // Holy Mage Specialist Card
                    _gameItemInstanceFactory.CreateItem(913, 1, 20), // Blue Magician Specialist Card
                    _gameItemInstanceFactory.CreateItem(914, 1, 20), // Dark Gunner Specialist Card
                    _gameItemInstanceFactory.CreateItem(4502, 1, 20), // Volcano Specialist Card
                    _gameItemInstanceFactory.CreateItem(4499, 1, 20), // Tide Lord Specialist Card
                    _gameItemInstanceFactory.CreateItem(4491, 1, 20), // Seer Specialist Card
                    _gameItemInstanceFactory.CreateItem(4487, 1, 20), // Archmage Specialist Card
                    _gameItemInstanceFactory.CreateItem(4583, 1, 20), // Voodoo Priest Specialist Card
                    _gameItemInstanceFactory.CreateItem(8523, 1, 20), // Gravity Specialist Card
                    _gameItemInstanceFactory.CreateItem(8714, 1, 20), // Fire Storm Specialist Card

                    
                };
                
                foreach (GameItemInstance item in mageItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 4:
                GameItemInstance[] martialArtistItems = 
                {
                    _gameItemInstanceFactory.CreateItem(4624, 1, 10, 8), // Frost Claw
                    _gameItemInstanceFactory.CreateItem(4632, 1, 10, 8), // Dragon Eye
                    _gameItemInstanceFactory.CreateItem(4640, 1, 10, 8), // Dragonslayer Armour
                    _gameItemInstanceFactory.CreateItem(4486, 1, 20), // Draconic Fist Specialist Card
                    _gameItemInstanceFactory.CreateItem(4485, 1, 20), // Mystic Arts Specialist Card
                    _gameItemInstanceFactory.CreateItem(4437, 1, 20), // Master Wolf Specialist Card
                    _gameItemInstanceFactory.CreateItem(4532, 1, 20), // Demon Warrior Specialist Card
                    _gameItemInstanceFactory.CreateItem(4580, 1, 20), // Flame Druid Specialist Card
                    _gameItemInstanceFactory.CreateItem(8524, 1, 20), // Hydraulic First Specialist Card
                    _gameItemInstanceFactory.CreateItem(8715, 1, 20)  // Thunderer Specialist Card
                };
                
                foreach (GameItemInstance item in martialArtistItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 5:
                GameItemInstance[] resistanceItems = 
                {
                    // gloves
                    _gameItemInstanceFactory.CreateItem(4931, 1, 10, 8), // Magmaros' Gloves
                    _gameItemInstanceFactory.CreateItem(4932, 1, 10, 8), // Valakus' Gloves
                    _gameItemInstanceFactory.CreateItem(4969, 1, 10, 8), // Sealed Hellord Gloves
                    _gameItemInstanceFactory.CreateItem(4967, 1, 20), // Sealed Heavenly Gloves
                    _gameItemInstanceFactory.CreateItem(4549, 1, 20), // Ancient Beast Gloves (Replica)
                    _gameItemInstanceFactory.CreateItem(4548, 1, 20), // Spirit King Gloves (Replica)
                    _gameItemInstanceFactory.CreateItem(4510, 1, 20), // Spirit King Gloves
                    _gameItemInstanceFactory.CreateItem(4509, 1, 20), // Ancient Beast Gloves
                    _gameItemInstanceFactory.CreateItem(4644, 1, 20), // Dragonlord Gloves
                    _gameItemInstanceFactory.CreateItem(4643, 1, 20), // Flying Dragon Gloves
                    
                    // shoes
                    _gameItemInstanceFactory.CreateItem(4933, 1, 6), // Flame Giant Boots
                    _gameItemInstanceFactory.CreateItem(4934, 1, 6), // Kertos' Boots
                    _gameItemInstanceFactory.CreateItem(4550, 1, 6), // Ancient Beast Shoes (Replica)
                    _gameItemInstanceFactory.CreateItem(4551, 1, 6), // Spirit King Shoes (Replica)
                    _gameItemInstanceFactory.CreateItem(4968, 1, 6), // Sealed Heavenly Shoes
                    _gameItemInstanceFactory.CreateItem(4970, 1, 6), // Sealed Hellord Shoes
                    _gameItemInstanceFactory.CreateItem(4976, 1, 6), // Zenas' Luxury High Heels
                    _gameItemInstanceFactory.CreateItem(4839, 1, 6), // Fernon's Shoes
                    _gameItemInstanceFactory.CreateItem(4512, 1, 6), // Spirit King Shoes
                    _gameItemInstanceFactory.CreateItem(4511, 1, 6), // Ancient Beast Shoes
                    _gameItemInstanceFactory.CreateItem(4646, 1, 6), // Dragonlord Shoes
                    _gameItemInstanceFactory.CreateItem(4645, 1, 6) // Light Dragon Bone Shoes
                };
                
                foreach (GameItemInstance item in resistanceItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 6:
                GameItemInstance[] jewelleryItems = 
                {
                    // Necklace
                    _gameItemInstanceFactory.CreateItem(4522, 1), // Beastheart Necklace
                    _gameItemInstanceFactory.CreateItem(4655, 1), // Draconian Lucky Chain
                    _gameItemInstanceFactory.CreateItem(4658, 1), // White Dragon Necklace
                    _gameItemInstanceFactory.CreateItem(4657, 1),  // Dragon Necklace
                    
                    // Ring
                    _gameItemInstanceFactory.CreateItem(4518, 1), // Orc Hero Ring
                    _gameItemInstanceFactory.CreateItem(4651, 1), // Heavenly Ring
                    _gameItemInstanceFactory.CreateItem(4654, 1), // Dragon Crystal Ring
                    _gameItemInstanceFactory.CreateItem(4653, 1), // Dragon Claw Ring
                    
                    // Bracelet
                    _gameItemInstanceFactory.CreateItem(4514, 1), // Spirit King's Bracelet
                    _gameItemInstanceFactory.CreateItem(4650, 1), // Triceratops Bone Bracelet
                    _gameItemInstanceFactory.CreateItem(4647, 1), // Carved Dragon Bracelet
                    _gameItemInstanceFactory.CreateItem(4648, 1)  // Dragon Crystal Bracelet
                };
                
                foreach (GameItemInstance item in jewelleryItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 7:
                GameItemInstance[] wingsItems =
                {
                    _gameItemInstanceFactory.CreateItem(1685, 999), // Angel Wings
                    _gameItemInstanceFactory.CreateItem(1686, 999), // Devil Wings
                    _gameItemInstanceFactory.CreateItem(5087, 999), // Fire Wings
                    _gameItemInstanceFactory.CreateItem(5203, 999), // Ice Wings
                    _gameItemInstanceFactory.CreateItem(5372, 999), // Titan Wings
                    _gameItemInstanceFactory.CreateItem(5431, 999), // Archangel Wings
                    _gameItemInstanceFactory.CreateItem(5432, 999), // Archdaemon Wings
                    _gameItemInstanceFactory.CreateItem(5498, 999), // Blazing Fire Wings
                    _gameItemInstanceFactory.CreateItem(5499, 999), // Frosty Ice Wings
                    _gameItemInstanceFactory.CreateItem(5553, 999), // Golden Wings
                    _gameItemInstanceFactory.CreateItem(5560, 999), // Onyx Wings
                    _gameItemInstanceFactory.CreateItem(5591, 999), // Fairy Wings
                    _gameItemInstanceFactory.CreateItem(5702, 999), // Zephyr Wings
                    _gameItemInstanceFactory.CreateItem(5800, 999), // Lightning Wings
                    _gameItemInstanceFactory.CreateItem(5837, 999), // Mega Titan Wings
                    _gameItemInstanceFactory.CreateItem(9176, 999), // Blade Wings
                    _gameItemInstanceFactory.CreateItem(9212, 999), // Crystal Wings
                    _gameItemInstanceFactory.CreateItem(9242, 999), // Petal Wings
                    _gameItemInstanceFactory.CreateItem(9546, 999), // Lunar Wings
                    _gameItemInstanceFactory.CreateItem(9594, 999), // Green Retro Wings
                    _gameItemInstanceFactory.CreateItem(9596, 999), // Pink Retro Wings
                    _gameItemInstanceFactory.CreateItem(9597, 999), // Yellow Retro Wings
                    _gameItemInstanceFactory.CreateItem(9598, 999), // Purple Retro Wings
                    _gameItemInstanceFactory.CreateItem(9599, 999), // Red Retro Wings
                    _gameItemInstanceFactory.CreateItem(9760, 999), // Magenta Retro Wings
                    _gameItemInstanceFactory.CreateItem(9776, 999), // Cyan Retro Wings
                    _gameItemInstanceFactory.CreateItem(9453, 999), // Eagle Wings
                    _gameItemInstanceFactory.CreateItem(9909, 999), // Tree Wings
                    _gameItemInstanceFactory.CreateItem(9999, 999), // Steampunk Wings
                    _gameItemInstanceFactory.CreateItem(13239, 999), // Purple Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13240, 999), // Black Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13241, 999), // Turquoise Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13242, 999), // Blue Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13243, 999), // Red Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13244, 999), // Green Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13245, 999), // Yellow Mecha Flame Wings
                    _gameItemInstanceFactory.CreateItem(13467, 999), // Christmas Tree Wings
                    _gameItemInstanceFactory.CreateItem(13524, 999), // Black Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13574, 999), // Red/Blue Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13575, 999), // Blue/Turquoise Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13576, 999), // Yellow/Turquoise Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13577, 999), // Red/Green Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13578, 999), // Green/Silver Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13579, 999), // Blue/Gold Gossamer Wings
                    _gameItemInstanceFactory.CreateItem(13580, 999)  // White/Magenta Gossamer Wings

                };

                foreach (GameItemInstance item in wingsItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 8:
                GameItemInstance[] fairyItems =
                {
                    // divines
                    _gameItemInstanceFactory.CreateItem(4129, 1), // Elkaim
                    _gameItemInstanceFactory.CreateItem(4130, 1), // Ladine
                    _gameItemInstanceFactory.CreateItem(4131, 1), // Rumial
                    _gameItemInstanceFactory.CreateItem(4132, 1), // Varik
                    
                    // act6 zenas
                    _gameItemInstanceFactory.CreateItem(4705, 1), // Zenas (Fire)
                    _gameItemInstanceFactory.CreateItem(4706, 1), // Zenas (Water)
                    _gameItemInstanceFactory.CreateItem(4707, 1), // Zenas (Light)
                    _gameItemInstanceFactory.CreateItem(4708, 1), // Zenas (Shadow)
                    
                    // act6 erenia
                    _gameItemInstanceFactory.CreateItem(4709, 1), // Erenia (Fire)
                    _gameItemInstanceFactory.CreateItem(4710, 1), // Erenia (Water)
                    _gameItemInstanceFactory.CreateItem(4711, 1), // Erenia (Light)
                    _gameItemInstanceFactory.CreateItem(4712, 1), // Erenia (Shadow)

                    // act6 fernon
                    _gameItemInstanceFactory.CreateItem(4713, 1), // Fernon (Fire)
                    _gameItemInstanceFactory.CreateItem(4714, 1), // Fernon (Water)
                    _gameItemInstanceFactory.CreateItem(4715, 1), // Fernon (Light)
                    _gameItemInstanceFactory.CreateItem(4716, 1), // Fernon (Shadow)
                };

                foreach (GameItemInstance item in fairyItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 9:
                GameItemInstance[] consumablesItems =
                {
                    _gameItemInstanceFactory.CreateItem(1120, 999), // Large Special Potion
                    _gameItemInstanceFactory.CreateItem(2187, 999), // Special Pet Food
                    _gameItemInstanceFactory.CreateItem(1011, 999), // Huge Recovery Potion
                    _gameItemInstanceFactory.CreateItem(1242, 999), // Divine Mana Potion
                    _gameItemInstanceFactory.CreateItem(1243, 999), // Divine Health Potion
                    _gameItemInstanceFactory.CreateItem(1244, 999), // Divine Recovery Potion
                    _gameItemInstanceFactory.CreateItem(1245, 999), // Basic SP Recovery Potion
                    _gameItemInstanceFactory.CreateItem(1246, 999), // Attack Potion
                    _gameItemInstanceFactory.CreateItem(1247, 999), // Defence Potion
                    _gameItemInstanceFactory.CreateItem(1248, 999), // Energy Potion
                    _gameItemInstanceFactory.CreateItem(1249, 999), // Experience Potion
                    _gameItemInstanceFactory.CreateItem(5675, 999), // Adventurer's Knapsack (Permanent)
                    _gameItemInstanceFactory.CreateItem(5676, 999), // Partner's Backpack (Permanent)
                    _gameItemInstanceFactory.CreateItem(5677, 999), // Pet Basket (Permanent)
                    _gameItemInstanceFactory.CreateItem(9143, 999), // Inventory Expansion Ticket (Permanent)
                    _gameItemInstanceFactory.CreateItem(1285, 999), // Guardian Angel's Blessing
                    _gameItemInstanceFactory.CreateItem(1286, 999), // Ancelloan's Blessing
                    _gameItemInstanceFactory.CreateItem(1362, 999), // Soulstone Blessing
                    _gameItemInstanceFactory.CreateItem(1296, 999), // Fairy Booster
                    _gameItemInstanceFactory.CreateItem(5370, 999), // Fairy Experience Potion
                    _gameItemInstanceFactory.CreateItem(1366, 999), // Point Initialisation Potion
                };
                
                foreach (GameItemInstance item in consumablesItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 10:
                GameItemInstance[] costumesItems =
                {
                    _gameItemInstanceFactory.CreateItem(5737, 999), // Pixie Costume Set
                    _gameItemInstanceFactory.CreateItem(9234, 999), // Wonderland Costume Set
                    _gameItemInstanceFactory.CreateItem(9789, 999), // Sailing Costume Set
                    _gameItemInstanceFactory.CreateItem(9817, 999), // Skeleton Costume Set
                    _gameItemInstanceFactory.CreateItem(9791, 999), // Snorkelling Costume Set
                    _gameItemInstanceFactory.CreateItem(9788, 999), // Rafting Costume Set
                    _gameItemInstanceFactory.CreateItem(5816, 999), // Ice Witch Costume Set
                    _gameItemInstanceFactory.CreateItem(5736, 999), // Easter Bunny Costume Set
                    _gameItemInstanceFactory.CreateItem(5789, 999), // Tropical Costume Set
                    _gameItemInstanceFactory.CreateItem(9263, 999), // Honeybee Costume Set
                    _gameItemInstanceFactory.CreateItem(9790, 999), // Scuba Diving Costume Set
                    _gameItemInstanceFactory.CreateItem(5051, 999), // Aqua Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5080, 999), // Santa Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5183, 999), // Black Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5184, 999), // Blue Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5185, 999), // Green Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5186, 999), // Red Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5187, 999), // Pink Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5188, 999), // Turquoise Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5189, 999), // Yellow Bushi Costume Set 
                    _gameItemInstanceFactory.CreateItem(5190, 999), // Classic Bushi Costume Set
                    _gameItemInstanceFactory.CreateItem(5267, 999), // Fluffy Rabbit Costume Set (m) 
                    _gameItemInstanceFactory.CreateItem(5268, 999), // Fluffy Rabbit Costume Set (f)
                    _gameItemInstanceFactory.CreateItem(5302, 999), // Oto-Fox Costume Set
                    _gameItemInstanceFactory.CreateItem(5486, 999), // Cuddly Tiger Costume Set
                    _gameItemInstanceFactory.CreateItem(5487, 999), // Snow White Tiger Costume Set
                    _gameItemInstanceFactory.CreateItem(5572, 999), // Illusionist's Costume Set
                    _gameItemInstanceFactory.CreateItem(5592, 999), // Groovy Beach Costume Set
                };
                
                foreach (GameItemInstance item in costumesItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 11:
                GameItemInstance[] mountsItems =
                {
                    _gameItemInstanceFactory.CreateItem(5714, 1) // Magic Sleigh with Red-nosed Reindeer
                };
                
                foreach (GameItemInstance item in mountsItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 12:
                GameItemInstance[] upgradeItems =
                {
                    _gameItemInstanceFactory.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstanceFactory.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstanceFactory.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstanceFactory.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstanceFactory.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstanceFactory.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstanceFactory.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstanceFactory.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstanceFactory.CreateItem(2283, 999), // Shining Green Soul
                    _gameItemInstanceFactory.CreateItem(2284, 999), // Shining Red Soul
                    _gameItemInstanceFactory.CreateItem(2285, 999), // Shining Blue Soul
                    _gameItemInstanceFactory.CreateItem(1363, 999), // Lower SP Protection Scroll
                    _gameItemInstanceFactory.CreateItem(1364, 999), // Higher SP Protection Scroll
                    _gameItemInstanceFactory.CreateItem(1365, 999), // Soul Revival Stone
                    _gameItemInstanceFactory.CreateItem(2511, 999), // Dragon Skin
                    _gameItemInstanceFactory.CreateItem(2512, 999), // Dragon Blood
                    _gameItemInstanceFactory.CreateItem(2513, 999), // Dragon Heart
                    _gameItemInstanceFactory.CreateItem(2514, 999), // Small Ruby of Completion
                    _gameItemInstanceFactory.CreateItem(2515, 999), // Small Sapphire of Completion
                    _gameItemInstanceFactory.CreateItem(2516, 999), // Small Obsidian of Completion
                    _gameItemInstanceFactory.CreateItem(2517, 999), // Small Topaz of Completion
                    _gameItemInstanceFactory.CreateItem(2518, 999), // Ruby of Completion
                    _gameItemInstanceFactory.CreateItem(2519, 999), // Sapphire of Completion
                    _gameItemInstanceFactory.CreateItem(2520, 999), // Obsidian of Completion
                    _gameItemInstanceFactory.CreateItem(2521, 999)  // Topaz of Completion
                };
                
                foreach (GameItemInstance item in upgradeItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
        }
        return new SaltyCommandResult(true, $"ItemPack successfully created!");
    }
}