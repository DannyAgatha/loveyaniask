using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Configuration;
using Qmmands;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.PacketHandling.Customization;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Beta Game Tester")]
[Description("Module related to Beta Game Tester commands.")]
[RequireAuthority(AuthorityType.GA)]
public class BetaGameTester : SaltyModuleBase
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IItemsManager _itemManager;
    private readonly IGameLanguageService _language;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IServerManager _serverManager;
    private readonly ISkillsManager _skillsManager;
    private readonly BaseSkillMartialArtist _baseSkillMartialArtist;

    public BetaGameTester(IItemsManager itemsManager, IGameLanguageService language, ISkillsManager skillsManager, IBattleEntityAlgorithmService algorithm, ICharacterAlgorithm characterAlgorithm,
        IReputationConfiguration reputationConfiguration, IServerManager serverManager, IGameItemInstanceFactory gameItemInstance, IRankingManager rankingManager, BaseSkillMartialArtist baseSkillMartialArtist)
    {
        _language = language;
        _itemManager = itemsManager;
        _skillsManager = skillsManager;
        _algorithm = algorithm;
        _characterAlgorithm = characterAlgorithm;
        _reputationConfiguration = reputationConfiguration;
        _serverManager = serverManager;
        _gameItemInstance = gameItemInstance;
        _rankingManager = rankingManager;
        _baseSkillMartialArtist = baseSkillMartialArtist;
    }

    [Command("gold")]
    [Description("Set player gold")]
    public async Task<SaltyCommandResult> SetGold([Description("Amount of gold.")] long gold)
    {
        IClientSession session = Context.Player;
        if (gold < 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        if (gold > _serverManager.MaxGold)
        {
            session.PlayerEntity.Gold = _serverManager.MaxGold;
            session.RefreshGold();
            return new SaltyCommandResult(true, $"Your gold: {session.PlayerEntity.Gold}");
        }

        session.PlayerEntity.Gold = gold;
        session.RefreshGold();
        return new SaltyCommandResult(true, $"Your gold: {gold}");
    }

    [Command("item")]
    [Description("Create an Item")]
    public async Task<SaltyCommandResult> CreateitemAsync(
        [Description("Item VNUM.")] short itemvnum,
        [Description("Amount.")] int amount)
    {
        IClientSession session = Context.Player;
        GameItemInstance newItem = _gameItemInstance.CreateItem(itemvnum, amount);
        await session.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, $"Created item: {_language.GetLanguage(GameDataType.Item, newItem.GameItem.Name, Context.Player.UserLanguage)}");
    }

    [Command("item")]
    [Description("Create an Item with rare and upgrade.")]
    public async Task<SaltyCommandResult> CreateitemAsync(
        [Description("Item VNUM.")] short itemvnum,
        [Description("Amount.")] int amount,
        [Description("Item's rare.")] sbyte rare,
        [Description("Item's upgrade.")] byte upgrade)
    {
        IClientSession session = Context.Player;

        GameItemInstance newItem = _gameItemInstance.CreateItem(itemvnum, amount, upgrade, rare);
        await session.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, $"Created item: {_language.GetLanguage(GameDataType.Item, newItem.GameItem.Name, Context.Player.UserLanguage)}");
    }

    [Command("position", "pos")]
    [Description("Outputs your current position")]
    public async Task<SaltyCommandResult> WhereAmI() => new SaltyCommandResult(true,
        $"MapId: {Context.Player.CurrentMapInstance?.MapId} | X: {Context.Player.PlayerEntity.PositionX} | Y: {Context.Player.PlayerEntity.PositionY} | Dir: {Context.Player.PlayerEntity.Direction}");

    [Command("splevel", "splvl")]
    [Description("Set player job level")]
    public async Task<SaltyCommandResult> SetSpLevel(
        [Description("SP job.")] byte spLevel)
    {
        if (spLevel == 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        IClientSession session = Context.Player;
        if (session.PlayerEntity.Specialist == null)
        {
            return new SaltyCommandResult(false, "You need to wear Specialist Card!");
        }

        session.PlayerEntity.Specialist.SpLevel = spLevel;
        session.PlayerEntity.Specialist.Xp = 0;
        session.RefreshLevel(_characterAlgorithm);
        session.LearnSpSkill(_skillsManager, _language);
        foreach (IBattleEntitySkill skill in session.PlayerEntity.Skills)
        {
            skill.LastUse = DateTime.UtcNow.AddDays(-1);
        }

        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(session.PlayerEntity.Family, _language);
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        return new SaltyCommandResult(true, "Specialist Card SP Level has been updated.");
    }

    [Command("jlevel", "joblvl", "joblevel", "jlvl")]
    [Description("Set player job level")]
    public async Task<SaltyCommandResult> SetJobLevel(
        [Description("Joblevel.")] byte jobLevel)
    {
        if (jobLevel == 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        IClientSession session = Context.Player;

        if (session.PlayerEntity.Class == ClassType.Adventurer && jobLevel > 20)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        session.PlayerEntity.JobLevel = jobLevel;
        session.PlayerEntity.JobLevelXp = 0;

        session.RefreshLevel(_characterAlgorithm);

        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(session.PlayerEntity.Family, _language);
        session.BroadcastEffectInRange(EffectType.JobLevelUp);

        if (session.PlayerEntity.Class == ClassType.MartialArtist)
        {
            var skills = _baseSkillMartialArtist.DefaultSkills
                .Concat(_baseSkillMartialArtist.PassiveSkills)
                .ToDictionary(s => s.SkillVNum, s => new CharacterSkill { SkillVNum = s.SkillVNum });

            foreach (KeyValuePair<int, CharacterSkill> pair in skills)
            {
                session.PlayerEntity.CharacterSkills[pair.Key] = pair.Value;
            }
        }
        else
        {
            session.PlayerEntity.CharacterSkills[(short)(200 + 20 * (byte)session.PlayerEntity.Class)] = new CharacterSkill
            {
                SkillVNum = (short)(200 + 20 * (byte)session.PlayerEntity.Class)
            };
            session.PlayerEntity.CharacterSkills[(short)(201 + 20 * (byte)session.PlayerEntity.Class)] = new CharacterSkill
            {
                SkillVNum = (short)(201 + 20 * (byte)session.PlayerEntity.Class)
            };
            session.PlayerEntity.CharacterSkills[236] = new CharacterSkill
            {
                SkillVNum = 236
            };
        }

        session.PlayerEntity.SkillComponent.SkillUpgrades.Clear();

        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.LearnAdventurerSkill(_skillsManager, _language);

        return new SaltyCommandResult(true, "Job Level has been updated.");
    }

    [Command("speed")]
    [Description("Set player speed")]
    public async Task<SaltyCommandResult> SetSpeed(
        [Description("Amount of speed (0-59).")]
        byte speed)
    {
        if (speed > 59 || speed == 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        IClientSession session = Context.Player;

        session.PlayerEntity.Speed = speed;
        session.SendCondPacket();
        session.PlayerEntity.IsCustomSpeed = true;
        return new SaltyCommandResult(true, $"Speed: {speed}");
    }

    [Command("speed")]
    [Description("Turn off your custom speed")]
    public async Task<SaltyCommandResult> SetSpeed()
    {
        Context.Player.PlayerEntity.IsCustomSpeed = false;
        Context.Player.PlayerEntity.RefreshCharacterStats();
        Context.Player.SendCondPacket();
        return new SaltyCommandResult(true);
    }

    [Command("reput")]
    [Description("Set reputation to the session")]
    public async Task<SaltyCommandResult> SetReput(
        [Description("Amount of reputation.")] long reput)
    {
        if (reput < 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        IClientSession session = Context.Player;

        session.PlayerEntity.Reput = reput;
        session.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);
        return new SaltyCommandResult(true, $"Reputation: {reput}");
    }

    [Command("class", "changeclass")]
    [Description("Set character class.")]
    public async Task<SaltyCommandResult> SetClass(
        [Description("0 - Adv, 1 - Sword, 2 - Archer, 3 - Mage, 4 - MA.")]
        string classType)
    {
        if (!Enum.TryParse(classType, out ClassType classt))
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        if (classt > ClassType.MartialArtist)
        {
            return new SaltyCommandResult(false, "This Class doesn't exist!");
        }

        IClientSession session = Context.Player;

        session.EmitEvent(new ChangeClassEvent { NewClass = classt, ShouldObtainBasicItems = false, ShouldObtainNewFaction = false });
        return new SaltyCommandResult(true, "Class has been changed.");
    }
    
    [Command("subclass", "changesubclass")]
    [Description("Set character subclass.")]
    public async Task<SaltyCommandResult> SetSubClass(
        [Description("1 - OathKeeper, 2 - CrimsonFury, 3 - CelestialPaladin, etc.")]
        string subClassType)
    {
        if (!Enum.TryParse(subClassType, out SubClassType subclass))
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }
        
        if (subclass > SubClassType.StealthShadow)
        {
            return new SaltyCommandResult(false, "This SubClass doesn't exist!");
        }
        
        IClientSession session = Context.Player;
        
        if (session.PlayerEntity.Level < 15 || session.PlayerEntity.JobLevel < 20)
        {
            return new SaltyCommandResult(false, "Your level is too low to change subclass.");
        }
        
        if (session.PlayerEntity.IsInGroup())
        {
            return new SaltyCommandResult(false, "You need to leave your group to change subclass.");
        }
        
        if (session.PlayerEntity.SubClass == subclass)
        {
            return new SaltyCommandResult(false, "You already have this subclass.");
        }
        
        await session.EmitEventAsync(new ChangeSubClassEvent
        {
            NewSubClass = subclass,
            ShouldObtainBasicItems = true,
            TierLevel = 1,
            TierExperience = 0
        });
        
        session.SendChatMessageNoPlayer($"Subclass has been changed to {subclass.ToString().AddSpacesToCamelCase()}.", ChatMessageColorType.Orange);
        
        return new SaltyCommandResult(true, $"Subclass has been changed to {subclass.ToString().AddSpacesToCamelCase()}.");
    }
    
    [Command("subclass-tier")]
    [Description("Change the character's subclass without resetting TierLevel.")]
    public async Task<SaltyCommandResult> ChangeSubClassWithTier(
        [Description("1 - OathKeeper, 2 - CrimsonFury, 3 - CelestialPaladin, etc.")]
        string subClassType)
    {
        if (!Enum.TryParse(subClassType, out SubClassType subclass))
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }
        
        if (subclass > SubClassType.StealthShadow)
        {
            return new SaltyCommandResult(false, "This SubClass doesn't exist!");
        }
        
        IClientSession session = Context.Player;
        
        if (session.PlayerEntity.Level < 15 || session.PlayerEntity.JobLevel < 20)
        {
            return new SaltyCommandResult(false, "Your level is too low to change subclass.");
        }
        
        if (session.PlayerEntity.IsInGroup())
        {
            return new SaltyCommandResult(false, "You need to leave your group to change subclass.");
        }
        
        if (session.PlayerEntity.SubClass == subclass)
        {
            return new SaltyCommandResult(false, "You already have this subclass.");
        }
        
        await session.EmitEventAsync(new ChangeSubClassEvent
        {
            NewSubClass = subclass,
            ShouldObtainBasicItems = true,
            TierLevel = session.PlayerEntity.TierLevel, 
            TierExperience = session.PlayerEntity.TierExperience
        });
        
        session.SendChatMessageNoPlayer($"Subclass changed to {subclass.ToString().AddSpacesToCamelCase()}.", ChatMessageColorType.Orange);
        
        return new SaltyCommandResult(true, $"Subclass changed to {subclass.ToString().AddSpacesToCamelCase()}.");
    }
    
    [Command("tierlevel", "settierlevel")]
    [Description("Set character Tier Level.")]
    public async Task<SaltyCommandResult> SetTierLevel(
        [Description("Tier Level to set (1-5).")]
        string tierLevel)
    {
        if (!byte.TryParse(tierLevel, out byte newTierLevel))
        {
            return new SaltyCommandResult(false, "Invalid Tier Level value!");
        }
        
        if (newTierLevel is < 1 or > 5)
        {
            return new SaltyCommandResult(false, "Tier Level must be between 1 and 5.");
        }
        
        IClientSession session = Context.Player;
        
        await session.EmitEventAsync(new ChangeSubClassEvent
        {
            NewSubClass = session.PlayerEntity.SubClass,
            TierLevel = newTierLevel,
            ShouldObtainBasicItems = false 
        });
        
        return new SaltyCommandResult(true, $"Tier Level has been set to {newTierLevel}.");
    }
    
    [Command("level", "lvl")]
    [Description("Set player level")]
    public async Task<SaltyCommandResult> SetLvl(
        [Description("Level.")] byte level, bool mates = false)
    {
        IClientSession session = Context.Player;

        if (level == 0)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        if (level == 150)
        {
            return new SaltyCommandResult(false, "Wrong value!");
        }

        session.PlayerEntity.Level = level;
        session.PlayerEntity.LevelXp = 0;
        session.PlayerEntity.RefreshCharacterStats();
        session.PlayerEntity.RefreshMaxHpMp(_algorithm);
        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;

        session.RefreshStat();
        session.RefreshStatInfo();
        session.RefreshStatChar();
        session.RefreshLevel(_characterAlgorithm);

        IFamily family = session.PlayerEntity.Family;

        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(family, _language);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);

        if (mates)
        {
            foreach (IMateEntity mateEntity in session.PlayerEntity.MateComponent.GetMates())
            {
                mateEntity.Level = level;
                mateEntity.Hp = mateEntity.MaxHp;
                mateEntity.Mp = mateEntity.MaxMp;
            }

            session.RefreshMateStats();
        }

        return new SaltyCommandResult(true, "Level has been updated.");
    }

    [Command("sex")]
    [Description("Change sex of character")]
    public async Task<SaltyCommandResult> ChangeGenderAsync()
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.Gender = session.PlayerEntity.Gender == GenderType.Female ? GenderType.Male : GenderType.Female;
        session.SendMsg(_language.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_SEX_CHANGED, session.UserLanguage), MsgMessageType.Middle);

        session.SendEqPacket();
        session.SendGenderPacket();
        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(session.PlayerEntity.Family, _language);
        session.BroadcastCMode();
        session.BroadcastEffect(EffectType.Transform, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        return new SaltyCommandResult(true, "Gender has been changed.");
    }

    [Command("heal")]
    [Description("Heal yourself.")]
    public async Task<SaltyCommandResult> HealAsync()
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
        
        session.PlayerEntity.BroadcastHeal(session.PlayerEntity.MaxHp);
        session.RefreshStat();

        return new SaltyCommandResult(true, "You have been healed.");
    }

    [Command("godmode")]
    [Description("Enable or disable godmode.")]
    public async Task<SaltyCommandResult> GodmodeAsync()
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.CheatComponent.HasGodMode = !session.PlayerEntity.CheatComponent.HasGodMode;
        session.SendChatMessage($"GODMODE: {(session.PlayerEntity.CheatComponent.HasGodMode ? "ON" : "OFF")}", ChatMessageColorType.Yellow);
        return new SaltyCommandResult(true);
    }

    [Command("zoom")]
    [Description("Camera zoom.")]
    public async Task<SaltyCommandResult> ZoomAsync(
        [Description("Zoom value.")] byte valueZoom)
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.SkillComponent.Zoom = valueZoom;
        session.RefreshZoom();
        return new SaltyCommandResult(true, $"Zoom updated: {valueZoom}");
    }

    [Command("clearchat")]
    [Description("Clear your chat")]
    public async Task<SaltyCommandResult> ClearchatAsync()
    {
        IClientSession session = Context.Player;

        for (int i = 0; i < 50; i++)
        {
            session.SendChatMessage(" ", ChatMessageColorType.Red);
        }

        return new SaltyCommandResult(true);
    }

    [Command("completed-ts")]
    [Description("Check completed Time-Spaces done by player")]
    public async Task<SaltyCommandResult> CompletedTs(IClientSession target)
    {
        if (!target.PlayerEntity.CompletedTimeSpaces.Any())
        {
            return new SaltyCommandResult(true, "Player didn't completed any Time-Space");
        }

        foreach (long tsId in target.PlayerEntity.CompletedTimeSpaces)
        {
            Context.Player.SendChatMessage($"Completed Time-Space: {tsId}", ChatMessageColorType.Yellow);
        }

        return new SaltyCommandResult(true);
    }
    
    [Command("classpack")]
    [Description("Create a kit of items")]
    public async Task<SaltyCommandResult> CreateClassPackAsync(
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
                    _gameItemInstance.CreateItem(7, 1, 10, 7), // Adventurer's Sword
                    _gameItemInstance.CreateItem(11, 1, 10, 7), // Adventurer's Catapult
                    _gameItemInstance.CreateItem(17, 1, 10, 7), // Warm Cloak Set
                    _gameItemInstance.CreateItem(900, 1, 20), // Pyjama Specialist Card
                    _gameItemInstance.CreateItem(907, 1, 20), // Chicken Specialist Card
                    _gameItemInstance.CreateItem(908, 1, 20), // Jajamaru Specialist Card
                    _gameItemInstance.CreateItem(4099, 1, 20), // Pirate Specialist Card
                    _gameItemInstance.CreateItem(4416, 1, 20), // Wedding Costume Specialist Card
                    _gameItemInstance.CreateItem(4562, 1, 20), // Angler Specialist Card
                    _gameItemInstance.CreateItem(4575, 1, 20) // Chef Specialist Card
                };
                
                foreach (GameItemInstance item in adventurerItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 1:
                GameItemInstance[] swordmanItems = 
                {
                    _gameItemInstance.CreateItem(4618, 1, 10, 8), // Dragonslayer
                    _gameItemInstance.CreateItem(4626, 1, 10, 8),// Dragon Crystal Crossbow
                    _gameItemInstance.CreateItem(4634, 1, 10, 8), // Dragonslayer Armour
                    _gameItemInstance.CreateItem(901, 1, 20), // Warrior Specialist Card
                    _gameItemInstance.CreateItem(902, 1, 20), // Ninja Specialist Card
                    _gameItemInstance.CreateItem(909, 1, 20), // Crusader Specialist Card
                    _gameItemInstance.CreateItem(910, 1, 20), // Berserker Specialist Card
                    _gameItemInstance.CreateItem(4500, 1, 20), // Gladiator Specialist Card
                    _gameItemInstance.CreateItem(4497, 1, 20), // Battle Monk Specialist Card
                    _gameItemInstance.CreateItem(4493, 1, 20), // Death Reaper Specialist Card
                    _gameItemInstance.CreateItem(4489, 1, 20), // Renegade Specialist Card
                    _gameItemInstance.CreateItem(4581, 1, 20), // Waterfall Berserker Specialist Card
                    _gameItemInstance.CreateItem(8521, 1, 20), // Dragon Knight Specialist Card,
                    _gameItemInstance.CreateItem(8712, 1, 20) // Stone Breaker Specialist Card
                };
                
                foreach (GameItemInstance item in swordmanItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 2:
                GameItemInstance[] archerItems = 
                {
                    _gameItemInstance.CreateItem(4620, 1, 10, 8), // Breath of Destruction
                    _gameItemInstance.CreateItem(4628, 1, 10, 8), // Dragon Bone Dagger
                    _gameItemInstance.CreateItem(4636, 1, 10, 8), // Dragon Hunter Uniform
                    _gameItemInstance.CreateItem(903, 1, 20), // Ranger Specialist Card
                    _gameItemInstance.CreateItem(904, 1, 20), // Assassin Specialist Card
                    _gameItemInstance.CreateItem(911, 1, 20), // Destroyer Specialist Card
                    _gameItemInstance.CreateItem(912, 1, 20), // Wild Keeper Specialist Card
                    _gameItemInstance.CreateItem(4501, 1, 20), // Fire Cannoneer Specialist Card
                    _gameItemInstance.CreateItem(4498, 1, 20), // Scout Specialist Card
                    _gameItemInstance.CreateItem(4492, 1, 20), // Demon Hunter Specialist Card
                    _gameItemInstance.CreateItem(4488, 1, 20), // Avenging Angel Specialist Card
                    _gameItemInstance.CreateItem(4582, 1, 20), // Sunchaser Specialist Card
                    _gameItemInstance.CreateItem(8522, 1, 20), // Blaster Specialist Card
                    _gameItemInstance.CreateItem(8713, 1, 20) // Fog Hunter Specialist Card
                };
                
                foreach (GameItemInstance item in archerItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 3:
                GameItemInstance[] mageItems = 
                {
                    _gameItemInstance.CreateItem(4622, 1, 10, 8), // Dragon Soul Wand
                    _gameItemInstance.CreateItem(4630, 1, 10, 8), // Freeze Spell Gun
                    _gameItemInstance.CreateItem(4638, 1, 10, 8), // Frost Scale Robe
                    _gameItemInstance.CreateItem(905, 1, 20), // Red Magician Specialist Card
                    _gameItemInstance.CreateItem(906, 1, 20), // Holy Mage Specialist Card
                    _gameItemInstance.CreateItem(913, 1, 20), // Blue Magician Specialist Card
                    _gameItemInstance.CreateItem(914, 1, 20), // Dark Gunner Specialist Card
                    _gameItemInstance.CreateItem(4502, 1, 20), // Volcano Specialist Card
                    _gameItemInstance.CreateItem(4499, 1, 20), // Tide Lord Specialist Card
                    _gameItemInstance.CreateItem(4491, 1, 20), // Seer Specialist Card
                    _gameItemInstance.CreateItem(4487, 1, 20), // Archmage Specialist Card
                    _gameItemInstance.CreateItem(4583, 1, 20), // Voodoo Priest Specialist Card
                    _gameItemInstance.CreateItem(8523, 1, 20), // Gravity Specialist Card
                    _gameItemInstance.CreateItem(8714, 1, 20) // Fire Storm Specialist Card
                };
                
                foreach (GameItemInstance item in mageItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 4:
                GameItemInstance[] martialArtistItems = 
                {
                    _gameItemInstance.CreateItem(4624, 1, 10, 8), // Frost Claw
                    _gameItemInstance.CreateItem(4632, 1, 10, 8), // Dragon Eye
                    _gameItemInstance.CreateItem(4640, 1, 10, 8), // Dragonslayer Armour
                    _gameItemInstance.CreateItem(4486, 1, 20), // Draconic Fist Specialist Card
                    _gameItemInstance.CreateItem(4485, 1, 20), // Mystic Arts Specialist Card
                    _gameItemInstance.CreateItem(4437, 1, 20), // Master Wolf Specialist Card
                    _gameItemInstance.CreateItem(4532, 1, 20), // Demon Warrior Specialist Card
                    _gameItemInstance.CreateItem(4580, 1, 20), // Flame Druid Specialist Card
                    _gameItemInstance.CreateItem(8524, 1, 20), // Hydraulic First Specialist Card
                    _gameItemInstance.CreateItem(8715, 1, 20) // Thunderer Specialist Card
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
                    _gameItemInstance.CreateItem(4931, 1, 10, 8), // Magmaros' Gloves
                    _gameItemInstance.CreateItem(4932, 1, 10, 8), // Valakus' Gloves
                    _gameItemInstance.CreateItem(4969, 1, 10, 8), // Sealed Hellord Gloves
                    _gameItemInstance.CreateItem(4967, 1, 20), // Sealed Heavenly Gloves
                    _gameItemInstance.CreateItem(4549, 1, 20), // Ancient Beast Gloves (Replica)
                    _gameItemInstance.CreateItem(4548, 1, 20), // Spirit King Gloves (Replica)
                    _gameItemInstance.CreateItem(4510, 1, 20), // Spirit King Gloves
                    _gameItemInstance.CreateItem(4509, 1, 20), // Ancient Beast Gloves
                    _gameItemInstance.CreateItem(4644, 1, 20), // Dragonlord Gloves
                    _gameItemInstance.CreateItem(4643, 1, 20), // Flying Dragon Gloves
                    
                    // shoes
                    _gameItemInstance.CreateItem(4933, 1, 6), // Flame Giant Boots
                    _gameItemInstance.CreateItem(4934, 1, 6), // Kertos' Boots
                    _gameItemInstance.CreateItem(4550, 1, 6), // Ancient Beast Shoes (Replica)
                    _gameItemInstance.CreateItem(4551, 1, 6), // Spirit King Shoes (Replica)
                    _gameItemInstance.CreateItem(4968, 1, 6), // Sealed Heavenly Shoes
                    _gameItemInstance.CreateItem(4970, 1, 6), // Sealed Hellord Shoes
                    _gameItemInstance.CreateItem(4976, 1, 6), // Zenas' Luxury High Heels
                    _gameItemInstance.CreateItem(4839, 1, 6), // Fernon's Shoes
                    _gameItemInstance.CreateItem(4512, 1, 6), // Spirit King Shoes
                    _gameItemInstance.CreateItem(4511, 1, 6), // Ancient Beast Shoes
                    _gameItemInstance.CreateItem(4646, 1, 6), // Dragonlord Shoes
                    _gameItemInstance.CreateItem(4645, 1, 6) // Light Dragon Bone Shoes
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
                    _gameItemInstance.CreateItem(4522, 1), // Beastheart Necklace
                    _gameItemInstance.CreateItem(4655, 1), // Draconian Lucky Chain
                    _gameItemInstance.CreateItem(4658, 1), // White Dragon Necklace
                    _gameItemInstance.CreateItem(4657, 1),  // Dragon Necklace
                    
                    // Ring
                    _gameItemInstance.CreateItem(4518, 1), // Orc Hero Ring
                    _gameItemInstance.CreateItem(4651, 1), // Heavenly Ring
                    _gameItemInstance.CreateItem(4654, 1), // Dragon Crystal Ring
                    _gameItemInstance.CreateItem(4653, 1), // Dragon Claw Ring
                    
                    // Bracelet
                    _gameItemInstance.CreateItem(4514, 1), // Spirit King's Bracelet
                    _gameItemInstance.CreateItem(4650, 1), // Triceratops Bone Bracelet
                    _gameItemInstance.CreateItem(4647, 1), // Carved Dragon Bracelet
                    _gameItemInstance.CreateItem(4648, 1)  // Dragon Crystal Bracelet
                };
                
                foreach (GameItemInstance item in jewelleryItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 7:
                GameItemInstance[] wingsItems =
                {
                    _gameItemInstance.CreateItem(1685, 999), // Angel Wings
                    _gameItemInstance.CreateItem(1686, 999), // Devil Wings
                    _gameItemInstance.CreateItem(5087, 999), // Fire Wings
                    _gameItemInstance.CreateItem(5203, 999), // Ice Wings
                    _gameItemInstance.CreateItem(5372, 999), // Titan Wings
                    _gameItemInstance.CreateItem(5431, 999), // Archangel Wings
                    _gameItemInstance.CreateItem(5432, 999), // Archdaemon Wings
                    _gameItemInstance.CreateItem(5498, 999), // Blazing Fire Wings
                    _gameItemInstance.CreateItem(5499, 999), // Frosty Ice Wings
                    _gameItemInstance.CreateItem(5553, 999), // Golden Wings
                    _gameItemInstance.CreateItem(5560, 999), // Onyx Wings
                    _gameItemInstance.CreateItem(5591, 999), // Fairy Wings
                    _gameItemInstance.CreateItem(5702, 999), // Zephyr Wings
                    _gameItemInstance.CreateItem(5800, 999), // Lightning Wings
                    _gameItemInstance.CreateItem(5837, 999), // Mega Titan Wings
                    _gameItemInstance.CreateItem(9176, 999), // Blade Wings
                    _gameItemInstance.CreateItem(9212, 999), // Crystal Wings
                    _gameItemInstance.CreateItem(9242, 999), // Petal Wings
                    _gameItemInstance.CreateItem(9546, 999), // Lunar Wings
                    _gameItemInstance.CreateItem(9594, 999), // Green Retro Wings
                    _gameItemInstance.CreateItem(9596, 999), // Pink Retro Wings
                    _gameItemInstance.CreateItem(9597, 999), // Yellow Retro Wings
                    _gameItemInstance.CreateItem(9598, 999), // Purple Retro Wings
                    _gameItemInstance.CreateItem(9599, 999), // Red Retro Wings
                    _gameItemInstance.CreateItem(9760, 999), // Magenta Retro Wings
                    _gameItemInstance.CreateItem(9776, 999), // Cyan Retro Wings
                    _gameItemInstance.CreateItem(9453, 999), // Eagle Wings
                    _gameItemInstance.CreateItem(9909, 999), // Tree Wings
                    _gameItemInstance.CreateItem(9999, 999), // Steampunk Wings
                    _gameItemInstance.CreateItem(13239, 999), // Purple Mecha Flame Wings
                    _gameItemInstance.CreateItem(13240, 999), // Black Mecha Flame Wings
                    _gameItemInstance.CreateItem(13241, 999), // Turquoise Mecha Flame Wings
                    _gameItemInstance.CreateItem(13242, 999), // Blue Mecha Flame Wings
                    _gameItemInstance.CreateItem(13243, 999), // Red Mecha Flame Wings
                    _gameItemInstance.CreateItem(13244, 999), // Green Mecha Flame Wings
                    _gameItemInstance.CreateItem(13245, 999), // Yellow Mecha Flame Wings
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
                    _gameItemInstance.CreateItem(4129, 1), // Elkaim
                    _gameItemInstance.CreateItem(4130, 1), // Ladine
                    _gameItemInstance.CreateItem(4131, 1), // Rumial
                    _gameItemInstance.CreateItem(4132, 1), // Varik
                    
                    // act6 zenas
                    _gameItemInstance.CreateItem(4705, 1), // Zenas (Fire)
                    _gameItemInstance.CreateItem(4706, 1), // Zenas (Water)
                    _gameItemInstance.CreateItem(4707, 1), // Zenas (Light)
                    _gameItemInstance.CreateItem(4708, 1), // Zenas (Shadow)
                    
                    // act6 erenia
                    _gameItemInstance.CreateItem(4709, 1), // Erenia (Fire)
                    _gameItemInstance.CreateItem(4710, 1), // Erenia (Water)
                    _gameItemInstance.CreateItem(4711, 1), // Erenia (Light)
                    _gameItemInstance.CreateItem(4712, 1), // Erenia (Shadow)

                    // act6 fernon
                    _gameItemInstance.CreateItem(4713, 1), // Fernon (Fire)
                    _gameItemInstance.CreateItem(4714, 1), // Fernon (Water)
                    _gameItemInstance.CreateItem(4715, 1), // Fernon (Light)
                    _gameItemInstance.CreateItem(4716, 1), // Fernon (Shadow)
                };

                foreach (GameItemInstance item in fairyItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 9:
                GameItemInstance[] consumablesItems =
                {
                    _gameItemInstance.CreateItem(1120, 999), // Large Special Potion
                    _gameItemInstance.CreateItem(2187, 999), // Special Pet Food
                    _gameItemInstance.CreateItem(1011, 999), // Huge Recovery Potion
                    _gameItemInstance.CreateItem(1242, 999), // Divine Mana Potion
                    _gameItemInstance.CreateItem(1243, 999), // Divine Health Potion
                    _gameItemInstance.CreateItem(1244, 999), // Divine Recovery Potion
                    _gameItemInstance.CreateItem(1245, 999), // Basic SP Recovery Potion
                    _gameItemInstance.CreateItem(1246, 999), // Attack Potion
                    _gameItemInstance.CreateItem(1247, 999), // Defence Potion
                    _gameItemInstance.CreateItem(1248, 999), // Energy Potion
                    _gameItemInstance.CreateItem(1249, 999), // Experience Potion
                    _gameItemInstance.CreateItem(5675, 999), // Adventurer's Knapsack (Permanent)
                    _gameItemInstance.CreateItem(5676, 999), // Partner's Backpack (Permanent)
                    _gameItemInstance.CreateItem(5677, 999), // Pet Basket (Permanent)
                    _gameItemInstance.CreateItem(9143, 999), // Inventory Expansion Ticket (Permanent)
                    _gameItemInstance.CreateItem(1285, 999), // Guardian Angel's Blessing
                    _gameItemInstance.CreateItem(1286, 999), // Ancelloan's Blessing
                    _gameItemInstance.CreateItem(1362, 999), // Soulstone Blessing
                    _gameItemInstance.CreateItem(1296, 999), // Fairy Booster
                    _gameItemInstance.CreateItem(5370, 999), // Fairy Experience Potion
                    _gameItemInstance.CreateItem(1366, 999), // Point Initialisation Potion
                };
                
                foreach (GameItemInstance item in consumablesItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 10:
                GameItemInstance[] costumesItems =
                {
                    _gameItemInstance.CreateItem(5737, 999), // Pixie Costume Set
                    _gameItemInstance.CreateItem(9234, 999), // Wonderland Costume Set
                    _gameItemInstance.CreateItem(9789, 999), // Sailing Costume Set
                    _gameItemInstance.CreateItem(9817, 999), // Skeleton Costume Set
                    _gameItemInstance.CreateItem(9791, 999), // Snorkelling Costume Set
                    _gameItemInstance.CreateItem(9788, 999), // Rafting Costume Set
                    _gameItemInstance.CreateItem(5816, 999), // Ice Witch Costume Set
                    _gameItemInstance.CreateItem(5736, 999), // Easter Bunny Costume Set
                    _gameItemInstance.CreateItem(5789, 999), // Tropical Costume Set
                    _gameItemInstance.CreateItem(9263, 999), // Honeybee Costume Set
                    _gameItemInstance.CreateItem(9790, 999), // Scuba Diving Costume Set
                    _gameItemInstance.CreateItem(5051, 999), // Aqua Bushi Costume Set
                    _gameItemInstance.CreateItem(5080, 999), // Santa Bushi Costume Set
                    _gameItemInstance.CreateItem(5183, 999), // Black Bushi Costume Set
                    _gameItemInstance.CreateItem(5184, 999), // Blue Bushi Costume Set
                    _gameItemInstance.CreateItem(5185, 999), // Green Bushi Costume Set
                    _gameItemInstance.CreateItem(5186, 999), // Red Bushi Costume Set
                    _gameItemInstance.CreateItem(5187, 999), // Pink Bushi Costume Set
                    _gameItemInstance.CreateItem(5188, 999), // Turquoise Bushi Costume Set
                    _gameItemInstance.CreateItem(5189, 999), // Yellow Bushi Costume Set 
                    _gameItemInstance.CreateItem(5190, 999), // Classic Bushi Costume Set
                    _gameItemInstance.CreateItem(5267, 999), // Fluffy Rabbit Costume Set (m) 
                    _gameItemInstance.CreateItem(5268, 999), // Fluffy Rabbit Costume Set (f)
                    _gameItemInstance.CreateItem(5302, 999), // Oto-Fox Costume Set
                    _gameItemInstance.CreateItem(5486, 999), // Cuddly Tiger Costume Set
                    _gameItemInstance.CreateItem(5487, 999), // Snow White Tiger Costume Set
                    _gameItemInstance.CreateItem(5572, 999), // Illusionist's Costume Set
                    _gameItemInstance.CreateItem(5592, 999), // Groovy Beach Costume Set
                };
                
                foreach (GameItemInstance item in costumesItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 11:
                GameItemInstance[] mountsItems =
                {
                    _gameItemInstance.CreateItem(5714, 1) // Magic Sleigh with Red-nosed Reindeer
                };
                
                foreach (GameItemInstance item in mountsItems)
                {
                    await session.AddNewItemToInventory(item, sendGiftIsFull: true);
                }
                break;
            
            case 12:
                GameItemInstance[] upgradeItems =
                {
                    _gameItemInstance.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstance.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstance.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstance.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstance.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstance.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstance.CreateItem(1030, 999), // Full Moon Crystal
                    _gameItemInstance.CreateItem(2282, 999), // Angel's Feather
                    _gameItemInstance.CreateItem(2283, 999), // Shining Green Soul
                    _gameItemInstance.CreateItem(2284, 999), // Shining Red Soul
                    _gameItemInstance.CreateItem(2285, 999), // Shining Blue Soul
                    _gameItemInstance.CreateItem(1363, 999), // Lower SP Protection Scroll
                    _gameItemInstance.CreateItem(1364, 999), // Higher SP Protection Scroll
                    _gameItemInstance.CreateItem(1365, 999), // Soul Revival Stone
                    _gameItemInstance.CreateItem(2511, 999), // Dragon Skin
                    _gameItemInstance.CreateItem(2512, 999), // Dragon Blood
                    _gameItemInstance.CreateItem(2513, 999), // Dragon Heart
                    _gameItemInstance.CreateItem(2514, 999), // Small Ruby of Completion
                    _gameItemInstance.CreateItem(2515, 999), // Small Sapphire of Completion
                    _gameItemInstance.CreateItem(2516, 999), // Small Obsidian of Completion
                    _gameItemInstance.CreateItem(2517, 999), // Small Topaz of Completion
                    _gameItemInstance.CreateItem(2518, 999), // Ruby of Completion
                    _gameItemInstance.CreateItem(2519, 999), // Sapphire of Completion
                    _gameItemInstance.CreateItem(2520, 999), // Obsidian of Completion
                    _gameItemInstance.CreateItem(2521, 999)  // Topaz of Completion
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