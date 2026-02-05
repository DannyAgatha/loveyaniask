using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsAPI.Data.Fish;
using WingsAPI.Data.Prestige;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Recipes;
using WingsEmu.DTOs.Skills;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Administrator")]
[Description("Module related to Administrator commands.")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorModule : SaltyModuleBase
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cards;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly ICharacterService _characterService;
    private readonly IMonsterEntityFactory _entity;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly SerializableGameServer _gameServer;
    private readonly IItemsManager _itemManager;
    private readonly IGameLanguageService _language;
    private readonly IServerManager _manager;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRecipeManager _recipeManager;
    private readonly IServerApiService _serverApiService;
    private readonly ISessionManager _sessionManager;
    private readonly ISkillsManager _skillManager;
    private readonly ISkillsManager _skillsManager;
    private readonly IFishManager _fishManager;
    private readonly FishConfiguration _fishConfiguration;

    public AdministratorModule(IServerManager manager, IGameLanguageService language, ISessionManager sessionManager,
        IItemsManager itemsManager, INpcMonsterManager npcMonsterManager, ISkillsManager skillManager, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory, IRecipeManager recipeManager,
        ICharacterAlgorithm characterAlgorithm, ICardsManager cards, IGameItemInstanceFactory gameItemInstanceFactory, IMonsterEntityFactory entity, ICharacterService characterService,
        ISkillsManager skillsManager, IServerApiService serverApiService, SerializableGameServer gameServer, IFishManager fishManager, FishConfiguration fishConfiguration)
    {
        _itemManager = itemsManager;
        _npcMonsterManager = npcMonsterManager;
        _skillManager = skillManager;
        _sessionManager = sessionManager;
        _manager = manager;
        _language = language;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
        _recipeManager = recipeManager;
        _characterAlgorithm = characterAlgorithm;
        _cards = cards;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _entity = entity;
        _characterService = characterService;
        _skillsManager = skillsManager;
        _serverApiService = serverApiService;
        _gameServer = gameServer;
        _fishManager = fishManager;
        _fishConfiguration = fishConfiguration;
    }

    [Command("removeitem")]
    [Description("Remove item from the target inventory")]
    public async Task<SaltyCommandResult> RemoveItem(IClientSession target, byte slot, InventoryType inventoryType, short? amount = null)
    {
        if (inventoryType == InventoryType.EquippedItems)
        {
            return new SaltyCommandResult(false, "Don't use it for this - use $remove-item-equipped");
        }

        InventoryItem item = target.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
        if (item?.ItemInstance == null)
        {
            return new SaltyCommandResult(false, "Item not found in this slot and type");
        }

        await target.RemoveItemFromInventory(item: item, amount: (short)(amount ?? item.ItemInstance.Amount));

        return new SaltyCommandResult(true, "Item has been removed from the player");
    }

    [Command("removeitem-equipped")]
    [Description("Remove equipped item from the target inventory")]
    public async Task<SaltyCommandResult> RemoveItemEquipped(IClientSession target, EquipmentType slot)
    {
        InventoryItem item = target.PlayerEntity.GetAllPlayerInventoryItems().FirstOrDefault(x
            => x?.ItemInstance != null && x.IsEquipped && x.ItemInstance.GameItem.EquipmentSlot == slot);

        if (item?.ItemInstance == null)
        {
            return new SaltyCommandResult(false, "Item not found in this slot.");
        }

        await target.RemoveItemFromInventory(item: item, isEquiped: true);

        if (target.ShouldSendAmuletPacket(item.ItemInstance.GameItem.EquipmentSlot))
        {
            target.SendEmptyAmuletBuffPacket();
        }

        return new SaltyCommandResult(true, "Item has been removed from the player");
    }

    [Command("skillscd")]
    public async Task<SaltyCommandResult> SkillsCd()
    {
        IClientSession session = Context.Player;

        foreach (CharacterSkill skill in session.PlayerEntity.CharacterSkills.Values)
        {
            session.SendChatMessage($"Skill {skill.Skill.Id} can be used: {session.PlayerEntity.SkillCanBeUsed(skill)} - LastUse: {skill.LastUse}", ChatMessageColorType.Yellow);
        }

        return new SaltyCommandResult(true);
    }

    [Command("check-monster")]
    public async Task<SaltyCommandResult> CheckMonster()
    {
        IClientSession session = Context.Player;
        (VisualType visualType, long id) = session.PlayerEntity.LastEntity;

        IBattleEntity entity = session.CurrentMapInstance.GetBattleEntity(visualType, id);
        if (entity is not IMonsterEntity monsterEntity)
        {
            return new SaltyCommandResult(false, "Entity is null or it's not the monster");
        }

        session.SendChatMessage($" Id: {monsterEntity.Id}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" IsStillAlive: {monsterEntity.IsStillAlive}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" SpawnDate: {monsterEntity.SpawnDate}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" Targets count: {monsterEntity.Targets.Count}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" Damagers count: {monsterEntity.Damagers.Count}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" NextTick: {monsterEntity.NextTick}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" NextAttackReady: {monsterEntity.NextAttackReady}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" Target: {monsterEntity.Target?.Type}|{monsterEntity.Target?.Id}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" IsApproachingTarget: {monsterEntity.IsApproachingTarget}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" ShouldFindNewTarget: {monsterEntity.ShouldFindNewTarget}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" FindNewPositionAroundTarget: {monsterEntity.FindNewPositionAroundTarget}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" ShouldRespawn: {monsterEntity.ShouldRespawn}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" IsMonsterSpawningMonstersForQuest: {monsterEntity.IsMonsterSpawningMonstersForQuest()}", ChatMessageColorType.DarkGrey);
        session.SendChatMessage($" Speed: {monsterEntity.Speed}", ChatMessageColorType.DarkGrey);

        session.SendChatMessage("[TARGETS]", ChatMessageColorType.Green);
        foreach (IBattleEntity target in monsterEntity.Targets)
        {
            session.SendChatMessage($"{target.Type}|{target.Id}", ChatMessageColorType.Yellow);
        }

        session.SendChatMessage("[DAMAGERS]", ChatMessageColorType.Green);
        foreach (IBattleEntity damager in monsterEntity.Damagers)
        {
            if (damager == null)
            {
                continue;
            }

            if (damager.MapInstance == null)
            {
                session.SendChatMessage($"Damager: {damager.Type}|{damager.Id} MapInstance null", ChatMessageColorType.Red);
                continue;
            }

            if (monsterEntity.MapInstance.Id != damager.MapInstance.Id)
            {
                session.SendChatMessage($"Damager: {damager.Type}|{damager.Id} MapInstance is not at the same map", ChatMessageColorType.Red);
                continue;
            }

            session.SendChatMessage($"Damager: {damager.Type}|{damager.Id}", ChatMessageColorType.Yellow);
        }

        return new SaltyCommandResult(true);
    }

    [Command("setsp")]
    public async Task<SaltyCommandResult> setsp(int amount)
    {
        Context.Player.PlayerEntity.SpPointsBasic = amount;
        Context.Player.RefreshSpPoint();
        return new SaltyCommandResult(true);
    }

    [Command("mapdance")]
    [Description("Dance!")]
    public async Task<SaltyCommandResult> MapDance()
    {
        Context.Player.CurrentMapInstance.IsDance = !Context.Player.CurrentMapInstance.IsDance;
        Context.Player.CurrentMapInstance.Broadcast(x => x.GenerateDance(Context.Player.CurrentMapInstance.IsDance));
        return new SaltyCommandResult(true, $"Map dancing: {Context.Player.CurrentMapInstance.IsDance}");
    }

    [Command("mapmusic")]
    [Description("Play music on map")]
    public async Task<SaltyCommandResult> MapMusic(short music)
    {
        Context.Player.CurrentMapInstance.MapMusic = music;
        Context.Player.CurrentMapInstance.Broadcast(x => x.GenerateMapMusic(music));
        return new SaltyCommandResult(true, $"Map music: {music}");
    }

    [Command("mapmusic")]
    [Description("Turn off the music on map")]
    public async Task<SaltyCommandResult> MapMusic()
    {
        Context.Player.CurrentMapInstance.MapMusic = null;
        return new SaltyCommandResult(true);
    }

    [Command("hlevel", "hlvl")]
    [Description("Set player hero level")]
    public async Task<SaltyCommandResult> SetHLvl(
        [Description("Hero level")] byte level)
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.HeroLevel = level;
        session.PlayerEntity.HeroXp = 0;
        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;

        session.RefreshStat();
        session.RefreshStatInfo();
        session.RefreshStatChar();
        session.RefreshLevel(_characterAlgorithm);
        return new SaltyCommandResult(true, "Hero level has been updated.");
    }

    [Command("debug")]
    [Description("toggles the debug mode on the client side")]
    public async Task<SaltyCommandResult> ToggleDebugModeAsync()
    {
        Context.Player.DebugMode = !Context.Player.DebugMode;
        Context.Player.SendChatMessage($"DEBUG_MODE: {(Context.Player.DebugMode ? "ON" : "OFF")}", ChatMessageColorType.Yellow);
        if (Context.Player.DebugMode)
        {
            Context.Player.SendPacket("debug");
        }

        return new SaltyCommandResult(true);
    }

    [Command("unlockalltitles")]
    [Description("Unlocks all titles available")]
    public async Task<SaltyCommandResult> AddAllTitles()
    {
        IClientSession session = Context.Player;

        IEnumerable<IGameItem> allTitles = _itemManager.GetItemsByType(ItemType.Title);

        foreach (IGameItem title in allTitles.Where(title => session.PlayerEntity.Titles.All(x => x.ItemVnum != title.Id)))
        {
            session.PlayerEntity.Titles.Add(new CharacterTitleDto
            {
                TitleId = _itemManager.GetTitleId(title.Id),
                ItemVnum = title.Id
            });
        }

        session.SendTitlePacket();
        return new SaltyCommandResult(true, "All titles have been unlocked.");
    }

    [Command("entities")]
    public async Task<SaltyCommandResult> Entities(byte range)
    {
        IReadOnlyList<IBattleEntity> entities = Context.Player.CurrentMapInstance.GetBattleEntitiesInRange(Context.Player.PlayerEntity.Position, range);
        foreach (IBattleEntity entity in entities)
        {
            Context.Player.SendChatMessage($"Entity - Type: {entity.Type.ToString()}, Id: {entity.Id}", ChatMessageColorType.Green);
        }

        Context.Player.SendChatMessage($"Entities: {entities.Count}", ChatMessageColorType.Red);
        return new SaltyCommandResult(true);
    }

    [Command("setitemdate")]
    public async Task<SaltyCommandResult> SetItemDate(short slot, short minutes)
    {
        IClientSession session = Context.Player;
        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (item?.ItemInstance == null)
        {
            return new SaltyCommandResult(false, "No item.");
        }

        item.ItemInstance.ItemDeleteTime = DateTime.UtcNow.AddMinutes(minutes);
        return new SaltyCommandResult(true, $"The item will disappear: {item.ItemInstance.ItemDeleteTime}");
    }

    [Command("check-pos")]
    [Description("Checks if the position is alright")]
    public async Task<SaltyCommandResult> CheckPosition()
    {
        Position position = Context.Player.PlayerEntity.Position;
        return new SaltyCommandResult(true, $"Position CanWalkAround: {Context.Player.CurrentMapInstance.CanWalkAround(position.X, position.Y).ToString()}");
    }

    [Command("nocd")]
    [Description("Enable/disable cooldown reduction from skills")]
    public async Task<SaltyCommandResult> NoCd()
    {
        IClientSession session = Context.Player;
        session.PlayerEntity.CheatComponent.HasNoCooldown = !session.PlayerEntity.CheatComponent.HasNoCooldown;

        return new SaltyCommandResult(true, $"No cooldown: {session.PlayerEntity.CheatComponent.HasNoCooldown}");
    }

    [Command("nolimit", "notargetlimit")]
    [Description("Enable/disable cooldown reduction from skills")]
    public async Task<SaltyCommandResult> NoTargetLimit()
    {
        IClientSession session = Context.Player;
        session.PlayerEntity.CheatComponent.HasNoTargetLimit = !session.PlayerEntity.CheatComponent.HasNoTargetLimit;

        return new SaltyCommandResult(true, $"No target limit: {session.PlayerEntity.CheatComponent.HasNoTargetLimit.ToString()}");
    }

    [Command("clearinv")]
    [Description("Remove all items from your inventory.")]
    public async Task<SaltyCommandResult> ClearInv()
    {
        IClientSession session = Context.Player;
        foreach (InventoryItem eqItem in session.PlayerEntity.EquippedItems)
        {
            if (eqItem?.ItemInstance == null)
            {
                continue;
            }

            session.PlayerEntity.TakeOffItem(eqItem.ItemInstance.GameItem.EquipmentSlot);
            session.PlayerEntity.RefreshEquipmentValues(eqItem.ItemInstance, true);
        }

        foreach (InventoryItem item in session.PlayerEntity.GetAllPlayerInventoryItems())
        {
            if (item?.ItemInstance == null)
            {
                continue;
            }


            session.PlayerEntity.RemoveItemFromSlotAndType(item.Slot, item.InventoryType, out InventoryItem removedItem);
            session.SendInventoryRemovePacket(removedItem);
        }

        Context.Player.PlayerEntity.MinilandObjects.Clear();
        Context.Player.RefreshEquipment();
        Context.Player.RefreshStatChar();
        Context.Player.RefreshStat();
        return new SaltyCommandResult(true, "Inventory has been cleared.");
    }

    [Command("recv")]
    [Description("Send recv packet to you.")]
    public async Task<SaltyCommandResult> Recv([Remainder] string packet)
    {
        Context.Player.SendPacket(packet);
        return new SaltyCommandResult(true);
    }

    [Command("recurrent-walk-bubble")]
    [Description("put a bubble on your head with your coordinates")]
    public async Task<SaltyCommandResult> WalkBubble([Description("The amount of minutes you want your bubble")] int minutes)
    {
        var tmp = new CancellationTokenSource(TimeSpan.FromMinutes(minutes));
        Task.Run(async () =>
        {
            while (!tmp.Token.IsCancellationRequested)
            {
                Context.Player.SendPacket(Context.Player.GenerateBubble($"X: {Context.Player.PlayerEntity.PositionX} | Y: {Context.Player.PlayerEntity.PositionY}"));
                await Task.Delay(500, tmp.Token);
            }
        });
        return new SaltyCommandResult(true);
    }

    [Command("haircolor")]
    [Description("Sets your hair color")]
    public async Task<SaltyCommandResult> HairColor(byte hairColor)
    {
        IClientSession session = Context.Player;
        if (!Enum.IsDefined(typeof(HairColorType), hairColor))
        {
            return new SaltyCommandResult(false, "The given color was not found.");
        }

        session.PlayerEntity.HairColor = (HairColorType)hairColor;
        session.BroadcastEq();
        return new SaltyCommandResult(true);
    }


    [Command("recvtarget")]
    [Description("Send recv packet to target.")]
    public async Task<SaltyCommandResult> RecvTarget(IClientSession target, [Remainder] string packet)
    {
        target.SendPacket(packet);
        return new SaltyCommandResult(true, $"Receiving packet: {packet}");
    }

    [Command("kick-all", "clear-channel")]
    [Description("Kick all players in the channel.")]
    public async Task<SaltyCommandResult> KickPlayers()
    {
        var tmp = _sessionManager.Sessions.ToList();
        foreach (IClientSession session in tmp)
        {
            if (session == Context.Player)
            {
                continue;
            }

            Context.Player.SendSuccessChatMessage($"[KICK_ALL] Kicking {session.PlayerEntity.Name}...");
            session.ForceDisconnect();
        }

        return new SaltyCommandResult(true, "All player has been kicked on the channel.");
    }

    [Command("flush-all", "flush-save")]
    [Description("Kick all players in the channel.")]
    public async Task<SaltyCommandResult> FlushAll()
    {
        var stopwatch = Stopwatch.StartNew();
        await _characterService.FlushCharacterSaves(new DbServerFlushCharacterSavesRequest());
        stopwatch.Stop();
        return new SaltyCommandResult(true, $"Flushed after {stopwatch.ElapsedMilliseconds.ToString()}ms");
    }

    [Command("recipe")]
    [Description("Get all items for recipe of item")]
    public async Task<SaltyCommandResult> Recipe(
        [Description("Item vnum to create")] short vNum)
    {
        IClientSession session = Context.Player;

        IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipeByProducedItemVnum(vNum);

        if (recipes == null)
        {
            return new SaltyCommandResult(false, "No recipe for given item.");
        }

        foreach (Recipe item in recipes)
        {
            foreach (RecipeItemDTO recipeItem in item.Items)
            {
                GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(recipeItem.ItemVNum, recipeItem.Amount);
                await session.AddNewItemToInventory(newItem);
            }
        }

        return new SaltyCommandResult(true, "Items have been added to your inventory.");
    }

    [Command("drop")]
    [Description("Drop the item on the floor")]
    public async Task<SaltyCommandResult> DropAsync(short vNum, int amount)
    {
        IClientSession session = Context.Player;

        var position = new Position(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY);
        await _eventPipeline.ProcessEventAsync(new DropMapItemEvent(session, session.CurrentMapInstance, position, vNum, amount));
        return new SaltyCommandResult(true, "The item dropped.");
    }

    [Command("sp")]
    [Description("Transform to SP instantly")]
    public async Task<SaltyCommandResult> SpecialistAsync()
    {
        IClientSession session = Context.Player;

        GameItemInstance sp = session.PlayerEntity.Specialist;
        if (sp == null)
        {
            return new SaltyCommandResult(false);
        }

        await session.EmitEventAsync(new SpTransformEvent
        {
            Specialist = sp
        });

        return new SaltyCommandResult(true);
    }

    [Command("sound")]
    public async Task<SaltyCommandResult> Sound(int value)
    {
        Context.Player.SendGuriPacket(19, 0, value);
        return new SaltyCommandResult(true);
    }

    [Command("sound")]
    public async Task<SaltyCommandResult> Sound(int value, IClientSession target)
    {
        if (target == null)
        {
            return new SaltyCommandResult(false);
        }

        target.SendGuriPacket(19, 0, value);
        return new SaltyCommandResult(true);
    }

    [Command("bufftarget")]
    public async Task<SaltyCommandResult> BuffTarget(short buffId)
    {
        IClientSession session = Context.Player;

        IBattleEntity target = session.CurrentMapInstance.GetBattleEntity(session.PlayerEntity.LastEntity.Item1, session.PlayerEntity.LastEntity.Item2);
        Card card = _cards.GetCardByCardId(buffId);

        if (card == null)
        {
            return new SaltyCommandResult(false, "Not a buff");
        }

        if (target == null)
        {
            return new SaltyCommandResult(false, "Target no exist!");
        }

        Buff buff = _buffFactory.CreateBuff(card.Id, session.PlayerEntity);
        await target.AddBuffAsync(buff);

        return new SaltyCommandResult(true,
            $"Added buff {_language.GetLanguage(GameDataType.Card, card.Name, session.UserLanguage)} for target ({target.Type.ToString()}|{target.Id})");
    }

    [Command("butcher")]
    public async Task<SaltyCommandResult> Butcher(byte range = 0)
    {
        IClientSession session = Context.Player;

        long goldBefore = session.PlayerEntity.Gold;
        int count = 0;

        IReadOnlyList<IMonsterEntity> entities = session.CurrentMapInstance.GetAliveMonsters();

        foreach (IMonsterEntity monster in entities
                     .Where(monster => range <= 0 || monster.Position.IsInAoeZone(session.PlayerEntity, range))
                     .Where(monster => monster.IsAlive()))
        {
            monster.PlayersDamage[session.PlayerEntity.Id] = monster.MaxHp;
            monster.Damagers.Add(session.PlayerEntity);
            
            monster.MapInstance.Broadcast(monster.GenerateOut());
            await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)
            {
                Killer = session.PlayerEntity,
                IsByCommand = true,
            });
            count++;
        }

        long goldAfter = session.PlayerEntity.Gold;
        long goldGained = goldAfter - goldBefore;

        return new SaltyCommandResult(true, $"Killed {count} monsters and gained {goldGained} gold");
    }


    [Command("kill")]
    [Description("Kills a player in the map you are.")]
    public async Task<SaltyCommandResult> Kill(
        [Description("Name of the character to kill")]
        string nickname,
        [Description("Should the players know that it was your fault?")]
        bool anonymous = true)
    {
        IClientSession target = Context.Player.PlayerEntity.MapInstance.Sessions.FirstOrDefault(s => s.CharacterName() == nickname);
        return BasicKillLogic(Context.Player.PlayerEntity, target.PlayerEntity, anonymous);
    }

    [Command("killtarget")]
    [Description("Kills a player in the map you are.")]
    public async Task<SaltyCommandResult> KillTarget(
        [Description("Should the players know that it was your fault?")]
        bool anonymous = true)
    {
        IClientSession session = Context.Player;
        IBattleEntity target = session.CurrentMapInstance.GetBattleEntity(session.PlayerEntity.LastEntity.Item1, session.PlayerEntity.LastEntity.Item2);
        return BasicKillLogic(session.PlayerEntity, target, anonymous);
    }

    private SaltyCommandResult BasicKillLogic(IBattleEntity issuer, IBattleEntity victim, bool anonymous) => anonymous ? AnonymousKill(victim) : PublicKill(issuer, victim);

    private SaltyCommandResult AnonymousKill(IBattleEntity suicidalEntity)
    {
        if (suicidalEntity == default)
        {
            return new SaltyCommandResult(false, "The target provided is not valid");
        }

        var algorithmResult = new DamageAlgorithmResult(int.MaxValue, HitType.Critical, true, false);
        _eventPipeline.ProcessEventAsync(new ApplyHitEvent(suicidalEntity, algorithmResult, new HitInformation(suicidalEntity, _skillsManager.GetSkill(299).GetInfo())));

        return new SaltyCommandResult(true);
    }

    private SaltyCommandResult PublicKill(IBattleEntity issuer, IBattleEntity victim)
    {
        if (issuer == default || victim == default)
        {
            return new SaltyCommandResult(false, "The target provided is not valid");
        }

        var algorithmResult = new DamageAlgorithmResult(int.MaxValue, HitType.Critical, true, false);
        _eventPipeline.ProcessEventAsync(new ApplyHitEvent(victim, algorithmResult, new HitInformation(issuer, _skillsManager.GetSkill(1049).GetInfo())));
        return new SaltyCommandResult(true);
    }


    [Command("map-respawn")]
    public async Task<SaltyCommandResult> MapRespawn(byte range = 0)
    {
        IClientSession session = Context.Player;

        int count = 0;
        IReadOnlyList<IMonsterEntity> entities = session.CurrentMapInstance.GetDeadMonsters();

        foreach (IMonsterEntity monster in entities.ToList())
        {
            if (range > 0 && !monster.Position.IsInAoeZone(session.PlayerEntity, range))
            {
                continue;
            }

            monster.Death = DateTime.MinValue;
            count++;
        }

        return new SaltyCommandResult(true, $"{count} monsters are now alive!");
    }

    [Command("cleardrops")]
    public async Task<SaltyCommandResult> ClearDrops(byte range = 0)
    {
        IClientSession session = Context.Player;

        int count = 0;
        IReadOnlyList<MapItem> drops = session.CurrentMapInstance.Drops;

        foreach (MapItem drop in drops)
        {
            if (range > 0 && !session.PlayerEntity.Position.IsInAoeZone(new Position(drop.PositionX, drop.PositionY), range))
            {
                continue;
            }

            count++;
            drop.BroadcastOut();
            session.CurrentMapInstance.RemoveDrop(drop.TransportId);
        }

        return new SaltyCommandResult(true, $"{count} drops has been removed from the map.");
    }

    [Command("removebuff")]
    [Description("Remove the given buff from the target")]
    public async Task<SaltyCommandResult> RemoveBuff(IClientSession target, int cardId, bool force = false)
    {
        IClientSession session = Context.Player;

        if (target == null)
        {
            return new SaltyCommandResult(false, "target is offline");
        }

        Buff tmp = target.PlayerEntity.BuffComponent.GetBuff(cardId);
        if (tmp == null)
        {
            return new SaltyCommandResult(false, $"Given target does not have Buff with cardId: {cardId}");
        }

        await target.PlayerEntity.RemoveBuffAsync(force, tmp);


        return new SaltyCommandResult(true);
    }

    [Command("sethp")]
    [Description("Set your character hp to selected value")]
    public async Task<SaltyCommandResult> SetHp(int hp, bool mates = false)
    {
        IClientSession session = Context.Player;
        if (hp == 0)
        {
            return new SaltyCommandResult(false);
        }

        session.PlayerEntity.Hp = hp;

        session.RefreshStat();
        session.RefreshStatInfo();

        if (!mates)
        {
            return new SaltyCommandResult(true);
        }

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            mate.Hp = hp;
        }

        session.RefreshMateStats();

        return new SaltyCommandResult(true);
    }

    [Command("setmp")]
    [Description("Set your character mp to selected value")]
    public async Task<SaltyCommandResult> SetMp(ushort mp, bool mates = false)
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.Mp = mp;

        session.RefreshStat();
        session.RefreshStatInfo();

        if (!mates)
        {
            return new SaltyCommandResult(true);
        }

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            mate.Mp = mp;
        }

        session.RefreshMateStats();

        return new SaltyCommandResult(true);
    }

    [Command("addskill")]
    [Description("Add skill.")]
    public async Task<SaltyCommandResult> AddSkillAsync(
        [Description("Skill VNUM.")] short skillVNum)
    {
        IClientSession session = Context.Player;
        SkillDTO skillinfo = _skillManager.GetSkill(skillVNum);

        if (skillinfo == null)
        {
            return new SaltyCommandResult(false, "The skill doesn't exist!");
        }

        if (skillinfo.Id < 200)
        {
            foreach (CharacterSkill skill in session.PlayerEntity.CharacterSkills.Select(s => s.Value))
            {
                if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.Id < 200)
                {
                    session.PlayerEntity.CharacterSkills.TryRemove(skill.SkillVNum, out CharacterSkill _);
                }
            }
        }
        else
        {
            if (session.PlayerEntity.CharacterSkills.ContainsKey(skillVNum))
            {
                return new SaltyCommandResult(true, "You have already this skill learnt.");
            }

            if (skillinfo.UpgradeSkill != 0)
            {
                CharacterSkill oldupgrade = session.PlayerEntity.CharacterSkills.Select(s => s.Value).FirstOrDefault(s =>
                    s.Skill.UpgradeSkill == skillinfo.UpgradeSkill && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                if (oldupgrade != null)
                {
                    session.PlayerEntity.CharacterSkills.TryRemove(oldupgrade.SkillVNum, out CharacterSkill _);
                }
            }
        }

        var newSkill = new CharacterSkill { SkillVNum = skillVNum };

        session.PlayerEntity.CharacterSkills[skillVNum] = newSkill;
        session.PlayerEntity.Skills.Add(newSkill);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.RefreshLevel(_characterAlgorithm);
        return new SaltyCommandResult(true, $"Skill {_language.GetLanguage(GameDataType.Skill, skillinfo.Name, session.UserLanguage)} has been added.");
    }

    [Command("removeskill")]
    [Description("Remove skill.")]
    public async Task<SaltyCommandResult> RemoveSkillAsync(
        [Description("Skill VNUM.")] short skillvnum)
    {
        IClientSession session = Context.Player;
        SkillDTO skillinfo = _skillManager.GetSkill(skillvnum);

        if (skillinfo == null)
        {
            return new SaltyCommandResult(false, "The skill doesn't exist!");
        }

        session.PlayerEntity.CharacterSkills.TryRemove(skillvnum, out CharacterSkill _);
        IBattleEntitySkill toRemove = session.PlayerEntity.Skills.FirstOrDefault(x => x.Skill.Id == skillvnum);
        session.PlayerEntity.Skills.Remove(toRemove);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.RefreshLevel(_characterAlgorithm);
        return new SaltyCommandResult(true, "Skill has been removed.");
    }


    [Command("online")]
    [Description("Show players in game")]
    public async Task<SaltyCommandResult> PlayersOnlineAsync()
    {
        IClientSession session = Context.Player;

        session.SendChatMessage("[Players on the server]", ChatMessageColorType.Red);
        foreach (IClientSession s in _sessionManager.Sessions)
        {
            session.SendChatMessage($"{s.PlayerEntity.Name} | AccID: {s.Account.Id} | CVersion: {s.ClientVersion}", ChatMessageColorType.LightPurple);
        }

        session.SendChatMessage($"Online: {_sessionManager.SessionsCount}", ChatMessageColorType.Red);
        return new SaltyCommandResult(true);
    }


    [Command("list-hw")]
    [Description("Show players in game")]
    public async Task<SaltyCommandResult> ListHardwareIdsAsync()
    {
        IClientSession session = Context.Player;

        session.SendChatMessage("[Players on the server]", ChatMessageColorType.Red);
        foreach (IClientSession s in _sessionManager.Sessions)
        {
            session.SendChatMessage($"{s.PlayerEntity.Name} | HWID: {s.HardwareId}", ChatMessageColorType.LightPurple);
        }

        session.SendChatMessage($"Online: {_sessionManager.SessionsCount}", ChatMessageColorType.Red);
        return new SaltyCommandResult(true);
    }

    [Command("stat")]
    [Description("Current server configuration")]
    public async Task<SaltyCommandResult> StatAsync()
    {
        IClientSession session = Context.Player;

        session.SendChatMessage("[Current server rates]", ChatMessageColorType.Red);
        session.SendChatMessage($"XP Rate: {_manager.MobXpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"JobXP Rate: {_manager.JobXpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"HeroXP Rate: {_manager.HeroXpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"FairyXP Rate: {_manager.FairyXpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"MateXP Rate: {_manager.MateXpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"FamilyXP Rate: {_manager.FamilyExpRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Reput Rate: {_manager.ReputRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Drop Rate: {_manager.MobDropRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"GoldDrop Rate: {_manager.GoldDropRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage($"Gold Rate: {_manager.GoldRate}", ChatMessageColorType.LightPurple);
        session.SendChatMessage("[Current server configuration]", ChatMessageColorType.Red);
        session.SendChatMessage($"EXP Event: {_manager.ExpEvent}", ChatMessageColorType.LightPurple);
        return new SaltyCommandResult(true, "");
    }

    [Command("mapinfo")]
    [Description("Current Map informations")]
    public async Task<SaltyCommandResult> DumpMapInformations()
    {
        IClientSession session = Context.Player;
        IMapInstance mapInstance = Context.Player.CurrentMapInstance;

        session.SendChatMessage("[MAP_INFORMATIONS]", ChatMessageColorType.Red);

        foreach (PropertyInfo i in typeof(IMapInstance).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(s => !s.PropertyType.IsClass
                         && !s.PropertyType.IsAbstract
                         && !s.PropertyType.IsInterface).OrderBy(s => s.Name))
        {
            session.SendChatMessage($"{i.Name} : {i.GetValue(mapInstance)}", ChatMessageColorType.LightPurple);
        }

        return new SaltyCommandResult(true, "");
    }

    [Command("targetInfo")]
    [Description("Current Map informations")]
    public async Task<SaltyCommandResult> DumpTargetInformation()
    {
        IClientSession session = Context.Player;
        IBattleEntity entity = session.CurrentMapInstance.GetBattleEntity(session.PlayerEntity.LastEntity.Item1, session.PlayerEntity.LastEntity.Item2);
        if (entity == null)
        {
            return new SaltyCommandResult(false);
        }

        int? monsterVnum = entity switch
        {
            IMonsterEntity monsterEntity => monsterEntity.MonsterVNum,
            INpcEntity npcEntity => npcEntity.MonsterVNum,
            IMateEntity mateEntity => mateEntity.MonsterVNum,
            _ => null
        };

        session.SendChatMessage("==========[ Target Info ]==========", ChatMessageColorType.Red);
        session.SendChatMessage($"Type: {entity.Type}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"Id: {entity.Id}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"Position: X - {entity.PositionX} Y - {entity.PositionY}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"MonsterVnum: {monsterVnum?.ToString() ?? "None"}", ChatMessageColorType.Yellow);

        return new SaltyCommandResult(true, "");
    }

    [Command("charInfo", "characterInformation")]
    [Description("Current Map informations")]
    public async Task<SaltyCommandResult> DumpCharacterInformation() => await DumpCharacterInformation(Context.Player.PlayerEntity.Name);

    [Command("charInfo", "characterInformation")]
    [Description("Current Map informations")]
    public async Task<SaltyCommandResult> DumpCharacterInformation(string name)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterName(name);
        IClientSession contextSession = Context.Player;

        contextSession.SendChatMessage("[CHARACTER_INFORMATION]", ChatMessageColorType.Red);

        foreach (PropertyInfo property in typeof(AccountDTO).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(prop => prop.PropertyType.IsPrimitive
                     || prop.PropertyType == typeof(string)
                     || prop.PropertyType == typeof(Guid)
                     || prop.PropertyType.IsEnum).OrderBy(prop => prop.Name))
        {
            object value = property.GetValue(session.Account);
            contextSession.SendChatMessage($"{property.Name} : {value}", ChatMessageColorType.LightPurple);
        }

        return new SaltyCommandResult(true, "");
    }

    /*public async Task<SaltyCommandResult> CheckCharacterInformation(string name)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterName(name);
        
    }*/

    [Command("buff")]
    [Description("Add given buff")]
    public async Task<SaltyCommandResult> BuffAsync(
        [Description("Buff vnum")] short vnum)
    {
        IClientSession session = Context.Player;

        Buff buff = _buffFactory.CreateBuff(vnum, session.PlayerEntity);
        if (buff == null)
        {
            return new SaltyCommandResult(false);
        }

        await session.PlayerEntity.AddBuffAsync(buff);

        return new SaltyCommandResult(true, $"Created buff: {_language.GetLanguage(GameDataType.Card, buff.Name, Context.Player.UserLanguage)}");
    }

    [Command("buff")]
    [Description("Add given buff")]
    public async Task<SaltyCommandResult> BuffAsync(
        [Description("Buff vnum")] short vnum,
        [Description("Buff duration in seconds")]
        int duration)
    {
        IClientSession session = Context.Player;

        Buff buff = _buffFactory.CreateBuff(vnum, session.PlayerEntity, TimeSpan.FromSeconds(duration));
        await session.PlayerEntity.AddBuffAsync(buff);

        return new SaltyCommandResult(true, $"Created buff: {_language.GetLanguage(GameDataType.Card, buff.Name, Context.Player.UserLanguage)}");
    }

    [Command("removebuff")]
    [Description("Removes given buff")]
    public async Task<SaltyCommandResult> RemoveBuffAsync(
        [Description("Buff vnum")] short vnum)
    {
        IClientSession session = Context.Player;

        if (!session.PlayerEntity.BuffComponent.HasBuff(vnum))
        {
            return new SaltyCommandResult(false, "You don't have this buff!");
        }

        Buff buff = session.PlayerEntity.BuffComponent.GetBuff(vnum);
        await session.PlayerEntity.RemoveBuffAsync(true, buff);

        return new SaltyCommandResult(true, $"Removed buff {_language.GetLanguage(GameDataType.Card, buff.Name, session.UserLanguage)}");
    }

    [Command("eff")]
    [Description("Show effect")]
    public async Task<SaltyCommandResult> EffAsync(
        [Description("Effect ID")] short vnum)
    {
        IClientSession session = Context.Player;

        session.BroadcastEffect(vnum, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        return new SaltyCommandResult(true);
    }

    [Command("refresh-c", "refresh-compliments", "rc")]
    [Description("Refresh your monthly compliments.")]
    public async Task<SaltyCommandResult> RefreshMonthlyCompliments([Description("Force refresh")] bool force = false)
    {
        Context.Player.EmitEvent(new ComplimentsMonthlyRefreshEvent { Force = force });
        return new SaltyCommandResult(true);
    }

    [Command("changechannel")]
    public async Task<SaltyCommandResult> ChangeChannel(int channelId)
    {
        if (channelId == _gameServer.ChannelId)
        {
            return new SaltyCommandResult(false, "It's the same channel");
        }

        if (channelId == 51 || _gameServer.ChannelType == GameChannelType.ACT_4)
        {
            return new SaltyCommandResult(false, "Use $act4/$act4leave command instead");
        }

        IClientSession session = Context.Player;

        GetChannelInfoResponse response = await _serverApiService.GetChannelInfo(new GetChannelInfoRequest
        {
            WorldGroup = _gameServer.WorldGroup,
            ChannelId = channelId
        });

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Channel doesn't exist");
        }

        IPlayerEntity player = session.PlayerEntity;

        await session.EmitEventAsync(new PlayerChangeChannelEvent(response.GameServer, ItModeType.ToPortAlveus, player.MapId, player.MapX, player.MapY));
        return new SaltyCommandResult(true);
    }

    [Command("checkmate")]
    public async Task<SaltyCommandResult> CheckMate()
    {
        IClientSession session = Context.Player;
        IMateEntity target = session.CurrentMapInstance.GetMateById(session.PlayerEntity.LastEntity.Item2);
        if (target == null)
        {
            return new SaltyCommandResult(false, "Mate doesn't exist.");
        }

        session.SendChatMessage($"Level: {target.Level}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"MonsterRaceType: {target.MonsterRaceType}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"AttackType: {target.AttackType}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"WeaponLevel: {target.WeaponLevel}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"WinfoValue: {target.WinfoValue}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"BaseLevel: {target.BaseLevel}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"GetModifier: {target.GetModifier()}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"CleanDamageMin: {target.CleanDamageMin}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"CleanDamageMax: {target.CleanDamageMax}", ChatMessageColorType.Yellow);
        session.SendChatMessage($"MateType: {target.MateType}", ChatMessageColorType.Yellow);

        return new SaltyCommandResult(true);
    }

    [Command("refreshmate")]
    public async Task<SaltyCommandResult> RefreshMate()
    {
        IClientSession session = Context.Player;
        IMateEntity target = session.CurrentMapInstance.GetMateById(session.PlayerEntity.LastEntity.Item2);
        if (target == null)
        {
            return new SaltyCommandResult(false, "Mate doesn't exist.");
        }

        target.RefreshStatistics();

        return new SaltyCommandResult(true, "Statistics refresh.");
    }

    [Command("instantfish")]
    [Description("Simula la pesca y recibe instantáneamente una recompensa de pez basada en el spot actual.")]
    public async Task<SaltyCommandResult> InstantFish()
    {
        IClientSession session = Context.Player;
        
        FishingSpotDto currentSpot = _fishManager.GetFishSpotByMapId(session.PlayerEntity.MapInstance.MapId);
        if (currentSpot == null)
        {
            return new SaltyCommandResult(false, "No estás en un lugar de pesca válido.");
        }
        
        var random = new Random();
        var possibleFishRewards = currentSpot.Rewards.Where(r => !r.IsMaterial).ToList();
        if (possibleFishRewards.Count == 0)
        {
            return new SaltyCommandResult(false, "No hay peces disponibles en este lugar de pesca.");
        }

        FishingRewardsDto selectedFishReward = possibleFishRewards[random.Next(possibleFishRewards.Count)];

        FishInfo fishDetails = _fishConfiguration.FishInfo[selectedFishReward.RewardsVnum];

        double lengthFish = random.NextDouble() * (fishDetails.MaxSize - fishDetails.MinSize) + fishDetails.MinSize;
        
        CharacterFishDto alreadyHaveTheFish = session.PlayerEntity.FishDto.FirstOrDefault(s => s.FishVnum == selectedFishReward.RewardsVnum);

        if (alreadyHaveTheFish != null)
        {
            alreadyHaveTheFish.Amount++;
            if (alreadyHaveTheFish.MaxLenght < lengthFish)
            {
                alreadyHaveTheFish.MaxLenght = lengthFish;
            }
        }
        else
        {
            
            session.PlayerEntity.FishDto.Add(new CharacterFishDto
            {
                Amount = 1,
                FishVnum = selectedFishReward.RewardsVnum,
                MaxLenght = lengthFish,
            });
        }
        
        GameItemInstance rewardItem = _gameItemInstanceFactory.CreateItem(selectedFishReward.RewardsVnum, 1);
        short slot = session.PlayerEntity.GetNextInventorySlot(rewardItem.GameItem.Type);
        InventoryType type = rewardItem.GameItem.Type;
        var inventoryItem = new InventoryItem
        {
            InventoryType = type,
            IsEquipped = false,
            ItemInstance = rewardItem,
            CharacterId = session.PlayerEntity.Id,
            Slot = slot
        };
        await session.EmitEventAsync(new InventoryAddItemEvent(inventoryItem, true, ChatMessageColorType.Green, true));
        
        session.PlayerEntity.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto
        {
            Vnum = selectedFishReward.RewardsVnum
        });
        
        return new SaltyCommandResult(true, $"Has recibido el pez {rewardItem.ItemVNum} de {currentSpot.MinLvl} - {currentSpot.MaxLvl}. ¡Pesca instantánea exitosa!");
    }

    [Command("complete-task-prestige", "ctp")]
    public async Task<SaltyCommandResult> CompletePrestigeTask()
    {
        IClientSession session = Context.Player;
        
        CharacterPrestigeDto prestige = session.PlayerEntity.CharacterPrestigeDto;

        if (prestige?.Tasks == null || prestige.Tasks.Count == 0)
        {
            return new SaltyCommandResult(false, "You have no active Prestige tasks.");
        }

        foreach (PrestigeTaskDto task in prestige.Tasks.Where(task => !task.Completed))
        {
            task.Progress = task.RequiredAmount;
        }

        session.SendInfo("All prestige tasks have been marked as completed.");
        return new SaltyCommandResult(true, "Tasks completed.");
    }
    
    [Command("set-prestige-level", "spl")]
    public async Task<SaltyCommandResult> SetPrestigeLevel(byte level)
    {
        IClientSession session = Context.Player;
        CharacterPrestigeDto prestige = session.PlayerEntity.CharacterPrestigeDto;

        if (prestige == null)
        {
            return new SaltyCommandResult(false, "Prestige system not initialised for this character.");
        }

        if (level > 10)
        {
            return new SaltyCommandResult(false, "Invalid prestige level. Must be between 1 and 100.");
        }

        prestige.CurrentPrestigeLevel = level;
        session.SendInfo($"Prestige level set to {level}.");
        return new SaltyCommandResult(true, $"Prestige level updated to {level}.");
    }
}