using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Character;
using WingsAPI.Data.Fish;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families;
using WingsEmu.Game.Fish;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Game Master")]
[Description("Module related to Game Master commands.")]
[RequireAuthority(AuthorityType.GM)]
public class CharacterModule : SaltyModuleBase
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly ICharacterService _characterService;
    private readonly IGameLanguageService _gameLanguage;
    private readonly SerializableGameServer _gameServer;
    private readonly IItemsManager _itemManager;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISessionManager _sessionManager;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly IFishManager _fishManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;

    public CharacterModule(SerializableGameServer gameServer, ICharacterAlgorithm characterAlgorithm, ISessionManager sessionManager,
        IItemsManager itemsManager, IReputationConfiguration reputationConfiguration, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration,
        IRankingManager rankingManager, IBuffFactory buffFactory, ICharacterService characterService, IFishManager fishManager, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _itemManager = itemsManager;
        _reputationConfiguration = reputationConfiguration;
        _gameLanguage = gameLanguage;
        _spPartnerConfiguration = spPartnerConfiguration;
        _rankingManager = rankingManager;
        _buffFactory = buffFactory;
        _characterService = characterService;
        _gameServer = gameServer;
        _sessionManager = sessionManager;
        _characterAlgorithm = characterAlgorithm;
        _fishManager = fishManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    private static bool ContainsAllItems(List<short> a, List<short> b) => a.All(b.Contains);

    [Command("unlockfishspot")]
    [Description("Add spot rewards fish to your character")]
    public async Task<SaltyCommandResult> UnlockAllFish(byte spot)
    {
        IClientSession session = Context.Player;

        var rewardsBySpot = new Dictionary<byte, short>
        {
            [0] = 9409,
            [1] = 9410,
            [2] = 9411,
            [3] = 9412,
            [4] = 9413,
            [5] = 9414,
            [6] = 9415,
            [7] = 9416,
            [8] = 9417,
            [9] = 9418,
            [10] = 9419
        };

        FishingSpotDto spotDto = _fishManager.GetFishSpotByMapId(session.PlayerEntity.MapInstance.MapId);
        if (spotDto == null)
        {
            return new SaltyCommandResult(false, "Spot doesnt exist try 0-10."); ;
        }

        foreach (FishingRewardsDto spotRewards in spotDto.Rewards.Where(s => !s.IsMaterial))
        {
            CharacterFishDto alreadyHaveTheFish = session.PlayerEntity.FishDto.FirstOrDefault(s => s.FishVnum == spotRewards.RewardsVnum);

            if (alreadyHaveTheFish != null)
            {
                continue;
            }

            alreadyHaveTheFish = new CharacterFishDto
            {
                Amount = 1,
                FishVnum = spotRewards.RewardsVnum,
                MaxLenght = 1,
            };
            session.PlayerEntity.FishDto.Add(alreadyHaveTheFish);
            session.SendPacket(session.GenerateFish2Packet(_itemManager, spotRewards.RewardsVnum, 1, alreadyHaveTheFish.Amount));

            foreach (FishingSpotDto rewards in _fishManager.GetAllFishSpotByIndex())
            {
                if (session.PlayerEntity.FishRewardsEarnedDto.Any(s => s.Vnum == rewardsBySpot[(byte)rewards.FishVnum]))
                {
                    continue;
                }

                var allFishRewards = rewards.Rewards.Where(s => !s.IsMaterial).Select(s => s.RewardsVnum).ToList();
                var currentFish = session.PlayerEntity.FishDto.Select(s => (short)s.FishVnum).ToList();

                if (!ContainsAllItems(allFishRewards, currentFish))
                {
                    continue;
                }

                GameItemInstance title = _gameItemInstanceFactory.CreateItem(rewardsBySpot[(byte)rewards.FishVnum]);
                short slotTitle = session.PlayerEntity.GetNextInventorySlot(title.GameItem.Type);
                InventoryType typeTitle = title.GameItem.Type;
                var inventoryItemTitle = new InventoryItem
                {
                    InventoryType = typeTitle,
                    IsEquipped = false,
                    ItemInstance = title,
                    CharacterId = session.PlayerEntity.Id,
                    Slot = slotTitle
                };

                session.PlayerEntity.Session.EmitEvent(new InventoryAddItemEvent(inventoryItemTitle, true, ChatMessageColorType.Green, true, MessageErrorType.Chat, slotTitle, typeTitle, false));
                session.PlayerEntity.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto
                {
                    Vnum = title.ItemVNum
                });
            }

            if (ContainsAllItems(_fishManager.GetAllRewardsFromEachSpotByIndex().Select(s => s.RewardsVnum).ToList(),
                session.PlayerEntity.FishDto.Select(s => (short)s.FishVnum).ToList()))
            {
                if (session.PlayerEntity.FishRewardsEarnedDto.All(s => s.Vnum != 9408))
                {
                    // title
                    GameItemInstance title = _gameItemInstanceFactory.CreateItem(9408);
                    short slotTitle = session.PlayerEntity.GetNextInventorySlot(title.GameItem.Type);
                    InventoryType typeTitle = title.GameItem.Type;
                    var inventoryItemTitle = new InventoryItem
                    {
                        InventoryType = typeTitle,
                        IsEquipped = false,
                        ItemInstance = title,
                        CharacterId = session.PlayerEntity.Id,
                        Slot = slotTitle
                    };

                    session.PlayerEntity.Session.EmitEvent(new InventoryAddItemEvent(inventoryItemTitle, true, ChatMessageColorType.Green, true, MessageErrorType.Chat, slotTitle, typeTitle, false));
                    session.PlayerEntity.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto
                    {
                        Vnum = title.ItemVNum
                    });
                }

                if (session.PlayerEntity.FishRewardsEarnedDto.All(s => s.Vnum != 2485))
                {
                    // book
                    GameItemInstance title = _gameItemInstanceFactory.CreateItem(2485);
                    short slotTitle = session.PlayerEntity.GetNextInventorySlot(title.GameItem.Type);
                    InventoryType typeTitle = title.GameItem.Type;
                    var inventoryItemTitle = new InventoryItem
                    {
                        InventoryType = typeTitle,
                        IsEquipped = false,
                        ItemInstance = title,
                        CharacterId = session.PlayerEntity.Id,
                        Slot = slotTitle
                    };

                    session.PlayerEntity.Session.EmitEvent(new InventoryAddItemEvent(inventoryItemTitle, true, ChatMessageColorType.Green, true, MessageErrorType.Chat, slotTitle, typeTitle, false));
                    session.PlayerEntity.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto()
                    {
                        Vnum = title.ItemVNum
                    });
                }
            }
        }

        return new SaltyCommandResult(true, "All fish spot has been unlocked.");
    }
    
    [Command("unlockAllFish")]
    [Description("Add each fish to your character")]
    public async Task<SaltyCommandResult> UnlockAllFish()
    {
        IClientSession session = Context.Player;
        foreach (IGameItem fish in _itemManager.GetItemsByType(ItemType.Fish).OrderBy(s => s.Id))
        {
            CharacterFishDto alreadyHaveTheFish = session.PlayerEntity.FishDto.FirstOrDefault(s => s.FishVnum == fish.Id);

            if (alreadyHaveTheFish != null)
            {
                continue;
            }

            alreadyHaveTheFish = new CharacterFishDto
            {
                Amount = 1,
                FishVnum = fish.Id,
                MaxLenght = 1,
            };
            session.PlayerEntity.FishDto.Add(alreadyHaveTheFish);
            session.SendPacket(session.GenerateFish2Packet(_itemManager, fish.Id, 1, alreadyHaveTheFish.Amount));
        }

        return new SaltyCommandResult(true, "All fish has been unlocked.");
    }

    [Command("char-stats")]
    [Description("Look others inventory")]
    public async Task<SaltyCommandResult> CharacterStats(IClientSession target)
    {
        IClientSession session = Context.Player;

        session.SendChatMessage($"Damage dealt: {target.PlayerEntity.LifetimeStats.TotalDamageDealt}", ChatMessageColorType.Green);
        session.SendChatMessage($"Food used: {target.PlayerEntity.LifetimeStats.TotalFoodUsed}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold dropped: {target.PlayerEntity.LifetimeStats.TotalGoldDropped}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold spent: {target.PlayerEntity.LifetimeStats.TotalGoldSpent}", ChatMessageColorType.Green);
        session.SendChatMessage($"Items used: {target.PlayerEntity.LifetimeStats.TotalItemsUsed}", ChatMessageColorType.Green);
        session.SendChatMessage($"Monsters killed: {target.PlayerEntity.LifetimeStats.TotalMonstersKilled}", ChatMessageColorType.Green);
        session.SendChatMessage($"Players killed: {target.PlayerEntity.LifetimeStats.TotalPlayersKilled}", ChatMessageColorType.Green);
        session.SendChatMessage($"Potions used: {target.PlayerEntity.LifetimeStats.TotalPotionsUsed}", ChatMessageColorType.Green);
        session.SendChatMessage($"Raids lost: {target.PlayerEntity.LifetimeStats.TotalRaidsLost}", ChatMessageColorType.Green);
        session.SendChatMessage($"Raids won: {target.PlayerEntity.LifetimeStats.TotalRaidsWon}", ChatMessageColorType.Green);
        session.SendChatMessage($"Skills casted: {target.PlayerEntity.LifetimeStats.TotalSkillsCasted}", ChatMessageColorType.Green);
        session.SendChatMessage($"Snacks used: {target.PlayerEntity.LifetimeStats.TotalSnacksUsed}", ChatMessageColorType.Green);
        session.SendChatMessage($"Total time online: {target.PlayerEntity.LifetimeStats.TotalTimeOnline}", ChatMessageColorType.Green);
        session.SendChatMessage($"TimeSpace won: {target.PlayerEntity.LifetimeStats.TotalTimespacesWon}", ChatMessageColorType.Green);
        session.SendChatMessage($"TimeSpace lost: {target.PlayerEntity.LifetimeStats.TotalTimespacesLost}", ChatMessageColorType.Green);
        session.SendChatMessage($"Deaths by monster: {target.PlayerEntity.LifetimeStats.TotalDeathsByMonster}", ChatMessageColorType.Green);
        session.SendChatMessage($"Deaths by player: {target.PlayerEntity.LifetimeStats.TotalDeathsByPlayer}", ChatMessageColorType.Green);
        session.SendChatMessage($"Instant Battle won: {target.PlayerEntity.LifetimeStats.TotalInstantBattleWon}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold earned in bazaar items: {target.PlayerEntity.LifetimeStats.TotalGoldEarnedInBazaarItems}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold spent in bazaar fees: {target.PlayerEntity.LifetimeStats.TotalGoldSpentInBazaarFees}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold spent in bazaar items: {target.PlayerEntity.LifetimeStats.TotalGoldSpentInBazaarItems}", ChatMessageColorType.Green);
        session.SendChatMessage($"Gold spent in npc shop: {target.PlayerEntity.LifetimeStats.TotalGoldSpentInNpcShop}", ChatMessageColorType.Green);

        return new SaltyCommandResult(true);
    }

    [Command("seeinv")]
    [Description("Look target equipped inventory")]
    public async Task<SaltyCommandResult> SeeInv(IClientSession target, byte equipmentType)
    {
        IClientSession session = Context.Player;

        if (target == null)
        {
            return new SaltyCommandResult(false);
        }

        if (!Enum.TryParse(equipmentType.ToString(), out EquipmentType eqType))
        {
            return new SaltyCommandResult(false, "Wrong eqType slot.");
        }

        InventoryItem inventory = target.PlayerEntity.GetInventoryItemFromEquipmentSlot(eqType);
        if (inventory == null)
        {
            return new SaltyCommandResult(false, "Target isn't wearing any item on this slot.");
        }

        if (inventory.ItemInstance.GameItem.EquipmentSlot == EquipmentType.Sp)
        {
            if (inventory.ItemInstance.GameItem.IsPartnerSpecialist)
            {
                session.SendPartnerSpecialistInfo(inventory.ItemInstance);
            }
            else
            {
                session.SendSpecialistCardInfo(inventory.ItemInstance, _characterAlgorithm);
            }

            return new SaltyCommandResult(true);
        }

        session.SendEInfoPacket(inventory.ItemInstance, _itemManager, _characterAlgorithm);
        return new SaltyCommandResult(true);
    }

    [Command("seeinv")]
    [Description("Look others inventory")]
    public async Task<SaltyCommandResult> SeeInv(IClientSession target)
    {
        IClientSession session = Context.Player;

        if (target == null)
        {
            return new SaltyCommandResult(false);
        }

        session.SendPacket(target.GenerateExtsPacket());
        session.SendTargetEq(target.PlayerEntity);
        session.SendPacket(target.GenerateGoldPacket());
        session.SendMsg("Remember, it's not your inventory!", MsgMessageType.MiddleYellow);

        return new SaltyCommandResult(true);
    }

    [Command("seeinv")]
    [Description("Return to your inventory")]
    public async Task<SaltyCommandResult> SeeInv()
    {
        IClientSession session = Context.Player;

        session.ShowInventoryExtensions();
        session.SendStartStartupInventory();
        session.RefreshGold();

        return new SaltyCommandResult(true);
    }

    [Command("size")]
    public async Task<SaltyCommandResult> Size(byte desiredSize)
    {
        Context.Player.PlayerEntity.ChangeSize(desiredSize);
        return new SaltyCommandResult(true);
    }

    [Command("shout")]
    [Description("Shout message to all players")]
    public async Task<SaltyCommandResult> ShoutAsync(
        [Description("Message")] [Remainder] string message)
    {
        await Context.Player.EmitEventAsync(new ChatShoutAdminEvent
        {
            Message = message
        });
        return new SaltyCommandResult(true, "");
    }

    [Command("gmmode")]
    [Description("Turn on/off message [GM_ONLY]")]
    public async Task<SaltyCommandResult> GmMode()
    {
        Context.Player.GmMode = !Context.Player.GmMode;
        Context.Player.SendChatMessage($"GM_MODE: {(Context.Player.GmMode ? "ON" : "OFF")}", ChatMessageColorType.Yellow);
        return new SaltyCommandResult(true, "");
    }

    [Command("invisible", "visible")]
    [Description("You become visible / invisible")]
    public async Task<SaltyCommandResult> Invisible()
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.CheatComponent.IsInvisible = !session.PlayerEntity.CheatComponent.IsInvisible;
        session.SendEqPacket();
        session.SendPacket(session.GenerateInvisible());

        if (session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.BroadcastOut(new ExceptSessionBroadcast(session));
            foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                session.Broadcast(mate.GenerateOut());
            }
        }
        else
        {
            foreach (IClientSession receiverSession in session.CurrentMapInstance.Sessions)
            {
                bool isAnonymous = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4)
                    && receiverSession.PlayerEntity.Faction != session.PlayerEntity.Faction && !receiverSession.IsGameMaster();

                receiverSession.SendTargetInPacket(session, _reputationConfiguration, _rankingManager.TopReputation, isAnonymous, true);
                receiverSession.SendTargetGidxPacket(session, session.PlayerEntity.Family, _gameLanguage);

                receiverSession.SendTargetTitInfoPacket(session);
                receiverSession.SendTargetConstBuffEffects(session.PlayerEntity);

                if (session.PlayerEntity.IsOnVehicle)
                {
                    continue;
                }

                foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
                {
                    mate.TeleportNearCharacter();
                    string inPacket = mate.GenerateIn(_gameLanguage, receiverSession.UserLanguage, _spPartnerConfiguration, isAnonymous);
                    receiverSession.SendPacket(inPacket);
                    receiverSession.SendTargetConstBuffEffects(mate);
                }
            }

            session.RefreshParty(_spPartnerConfiguration);
        }

        return new SaltyCommandResult(true, $"Invisibility: {session.PlayerEntity.CheatComponent.IsInvisible}");
    }

    [Command("addtitle", "unlockTitle")]
    [Description("Add the given title to your character")]
    public async Task<SaltyCommandResult> AddTitle(
        [Description("Title Vnum you want")] short titleVnum)
    {
        IClientSession session = Context.Player;
        if (session.PlayerEntity.Titles.Any(s => s.ItemVnum == titleVnum))
        {
            return new SaltyCommandResult(false, "Title already unlocked.");
        }

        session.PlayerEntity.Titles.Add(new CharacterTitleDto { ItemVnum = titleVnum, TitleId = _itemManager.GetTitleId(titleVnum) });
        session.SendTitlePacket();
        return new SaltyCommandResult(true, "Title has been unlocked.");
    }

    [Command("info")]
    [Description("Information about player")]
    public async Task<SaltyCommandResult> InfoAsync() => await InfoAsync(Context.Player.PlayerEntity.Name);

    [Command("vfx")]
    [Description("VFX for GameMaster and Blowa - type GM or Blowa")]
    public async Task<SaltyCommandResult> VfxAsync(string vfx)
    {
        if (string.IsNullOrEmpty(vfx))
        {
            return new SaltyCommandResult(false, "VFX is empty");
        }

        IClientSession session = Context.Player;

        switch (vfx.ToLower())
        {
            case "gamemaster":
            case "gm":

                await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(20001, session.PlayerEntity, TimeSpan.FromHours(1)));

                break;
            case "blowa":

                if (session.PlayerEntity.Name != "Blowa")
                {
                    return new SaltyCommandResult(false, "You're not Blowa wrr...");
                }

                await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(20000, session.PlayerEntity, TimeSpan.FromHours(1)));

                break;
        }

        return new SaltyCommandResult(true);
    }

    [Command("info")]
    [Description("Information about player")]
    public async Task<SaltyCommandResult> InfoAsync(
        [Description("Player's nickname")] string targetName)
    {
        IClientSession session = Context.Player;

        IClientSession target = _sessionManager.GetSessionByCharacterName(targetName);
        if (target != null)
        {
            session.SendChatMessage($"      [Session information: {target.PlayerEntity.Name}]", ChatMessageColorType.Red);
            session.SendChatMessage($"SessionID: {target.SessionId}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"AccountID: {target.Account.Id}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Master AccountID: {target.Account.MasterAccountId.ToString()}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Account Name: {target.Account.Name}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"IP Address: {target.IpAddress}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Client Version: {target.ClientVersion}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"HardwareID: {target.HardwareId}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Channel: {_gameServer.ChannelId}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Language: {target.UserLanguage}", ChatMessageColorType.LightPurple);
            session.SendChatMessage("      [Character information]", ChatMessageColorType.Red);
            session.SendChatMessage($"CharacterID: {target.PlayerEntity.Id}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Name: {target.PlayerEntity.Name}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Level: {target.PlayerEntity.Level}(+{target.PlayerEntity.HeroLevel}) | XP: {target.PlayerEntity.LevelXp} | HeroXP: {target.PlayerEntity.HeroXp}",
                ChatMessageColorType.LightPurple);
            session.SendChatMessage($"JobLevel: {target.PlayerEntity.JobLevel} | JobXP: {target.PlayerEntity.JobLevelXp}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Position: MapID: {target.PlayerEntity.MapId} X: {target.PlayerEntity.PositionX} Y: {target.PlayerEntity.PositionY}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Speed: {target.PlayerEntity.Speed}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Gold: {target.PlayerEntity.Gold}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"Faction: {target.PlayerEntity.Faction}", ChatMessageColorType.LightPurple);
            session.SendChatMessage($"HP: {target.PlayerEntity.Hp}/{target.PlayerEntity.MaxHp} MP: {target.PlayerEntity.Mp}/{target.PlayerEntity.MaxMp}", ChatMessageColorType.LightPurple);
            IFamily family = target.PlayerEntity.Family;
            session.SendChatMessage($"Family: {(family != null ? family.Name : "None")}", ChatMessageColorType.LightPurple);
            return new SaltyCommandResult(true);
        }

        DbServerGetCharacterResponse getCharacter = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
        {
            CharacterName = targetName
        });

        if (getCharacter?.CharacterDto == null)
        {
            return new SaltyCommandResult(false, "Character not found in database");
        }

        CharacterDTO character = getCharacter.CharacterDto;

        session.SendChatMessage($"AccountID: {character.AccountId}", ChatMessageColorType.LightPurple);
        session.SendChatMessage("      [Character information]", ChatMessageColorType.Red);
        session.SendChatMessage($"CharacterID: {character.Id}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Name: {character.Name}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Level: {character.Level}(+{character.HeroLevel}) | XP: {character.LevelXp} | HeroXP: {character.HeroXp}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"JobLevel: {character.JobLevel} | JobXP: {character.JobLevelXp}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Last saved position: MapID: {character.MapId} X: {character.MapX} Y: {character.MapY}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Gold: {character.Gold}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Faction: {character.Faction}", ChatMessageColorType.LightPurple);

        if (character.Inventory != null)
        {
            session.SendChatMessage("      [Equipment information]", ChatMessageColorType.Red);
            foreach (CharacterInventoryItemDto eq in character.Inventory)
            {
                if (eq?.ItemInstance == null)
                {
                    continue;
                }

                session.SendChatMessage("-=-=-=-=-=-=-=-", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Slot: {eq.Slot}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- ItemVnum: {eq.ItemInstance.ItemVNum}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Amount: {eq.ItemInstance.Amount}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Serial (only eq.): {eq.ItemInstance.SerialTracker.ToString()}", ChatMessageColorType.Yellow);
            }
        }

        if (character.EquippedStuffs != null)
        {
            session.SendChatMessage("      [Equipment equipped information]", ChatMessageColorType.Red);
            foreach (CharacterInventoryItemDto eq in character.EquippedStuffs)
            {
                if (eq?.ItemInstance == null)
                {
                    continue;
                }

                session.SendChatMessage("-=-=-=-=-=-=-=-", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Slot: {eq.Slot}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- ItemVnum: {eq.ItemInstance.ItemVNum}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Amount: {eq.ItemInstance.Amount}", ChatMessageColorType.Yellow);
                session.SendChatMessage($"- Serial (only eq.): {eq.ItemInstance.SerialTracker.ToString()}", ChatMessageColorType.Yellow);
            }
        }

        return new SaltyCommandResult(true);
    }

    [Command("channel")]
    [Description("Get channel id")]
    public async Task<SaltyCommandResult> GetChannelNumber(string characterName)
    {
        ClusterCharacterInfo tmp = _sessionManager.GetOnlineCharacterByName(characterName);
        if (tmp == null)
        {
            return new SaltyCommandResult(false, $"Could not find {characterName} in online players");
        }

        return new SaltyCommandResult(true, $"[ONLINE] {characterName} is on channel {tmp.ChannelId}");
    }

    [Command("morph")]
    [Description("Transform into any monster")]
    public async Task<SaltyCommandResult> MorphAsync(
        [Description("Morph VNUM")] ushort morphVnum,
        [Description("Upgrade")] byte upgrade,
        [Description("Morph design")] byte morphDesign)
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.IsMorphed = true;
        session.PlayerEntity.Morph = morphVnum;
        session.PlayerEntity.MorphUpgrade = upgrade;
        session.PlayerEntity.MorphUpgrade2 = morphDesign;
        session.BroadcastCMode();
        return new SaltyCommandResult(true, "");
    }

    [Command("morph")]
    [Description("Disable morph")]
    public async Task<SaltyCommandResult> MorphAsync()
    {
        IClientSession session = Context.Player;

        await session.EmitEventAsync(new GetDefaultMorphEvent());
        session.PlayerEntity.IsMorphed = false;
        session.SendCondPacket();
        session.RefreshLevel(_characterAlgorithm);
        session.BroadcastCMode();
        return new SaltyCommandResult(true, "");
    }
}