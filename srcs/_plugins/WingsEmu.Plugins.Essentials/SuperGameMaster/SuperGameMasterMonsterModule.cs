using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("QuestHelpingModule")]
[Description("Module related to Miniland commands.")]
[RequireAuthority(AuthorityType.SGM)]
public class SuperGameMasterMonsterModule : SaltyModuleBase
{
    private readonly IMonsterEntityFactory _entity;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _language;
    private readonly INpcMonsterManager _npcMonsterManager;

    public SuperGameMasterMonsterModule(IMonsterEntityFactory entity, INpcMonsterManager npcMonsterManager, IGameLanguageService language, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _entity = entity;
        _npcMonsterManager = npcMonsterManager;
        _language = language;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }


    [Command("quest-mob")]
    [Description("Spawn monster with given vnum")]
    public async Task<SaltyCommandResult> SpawnMob(
        [Description("Mob vnum")] short vnum,
        [Description("Amount")] byte amount,
        [Description("Can move (false, true)")]
        bool canMove = false, bool isHostile = false)
    {
        IClientSession session = Context.Player;

        if (amount > 20)
        {
            return new SaltyCommandResult(false, "Amount can not exceed 20");
        }

        IMonsterData monster = _npcMonsterManager.GetNpc(vnum);

        if (monster == null)
        {
            return new SaltyCommandResult(false, "Monster doesn't exist!");
        }

        for (int i = 0; i < amount; i++)
        {
            IMonsterEntity monsterEntity = _entity.CreateMonster(monster, Context.Player.CurrentMapInstance, new MonsterEntityBuilder
            {
                IsWalkingAround = canMove,
                IsHostile = isHostile
            });
            monsterEntity.ChangePosition(Context.Player.PlayerEntity.Position);
            monsterEntity.FirstX = Context.Player.PlayerEntity.Position.X;
            monsterEntity.FirstY = Context.Player.PlayerEntity.Position.Y;
            await monsterEntity.EmitEventAsync(new MapJoinMonsterEntityEvent(monsterEntity));
        }

        return new SaltyCommandResult(true, $"Created monster: {_language.GetLanguage(GameDataType.NpcMonster, monster.Name, Context.Player.UserLanguage)}");
    }

    [Command("quest-item")]
    [Description("Create an Item")]
    public async Task<SaltyCommandResult> CreateitemAsync(
        [Description("Item VNUM.")] short itemvnum,
        [Description("Amount.")] int amount)
    {
        IClientSession session = Context.Player;
        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(itemvnum, amount);
        await session.AddNewItemToInventory(newItem);
        return new SaltyCommandResult(true, $"Created item: {_language.GetLanguage(GameDataType.Item, newItem.GameItem.Name, Context.Player.UserLanguage)}");
    }

    [Command("quest-mail")]
    [Description("Send gift mail to someone")]
    public async Task<SaltyCommandResult> ParcelAsync(IClientSession target,
        [Description("Item VNUM.")] int itemVnum,
        [Description("Amount.")] int amount)
    {
        IClientSession session = Context.Player;
        if (target == null)
        {
            return new SaltyCommandResult(false, "Player is offline.");
        }

        GameItemInstance item = _gameItemInstanceFactory.CreateItem(itemVnum, amount);
        await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, target.PlayerEntity.Id, MailGiftType.Normal, item));
        return new SaltyCommandResult(true, "Parcel has been sent.");
    }
}