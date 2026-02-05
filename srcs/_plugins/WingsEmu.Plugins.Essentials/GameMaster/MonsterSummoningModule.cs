// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("MonsterSummoning")]
[Description("Module related to Monster Summoning commands.")]
[RequireAuthority(AuthorityType.CM)]
public class MonsterSummoningModule : SaltyModuleBase
{
    private readonly IMonsterEntityFactory _entity;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _language;
    private readonly INpcMonsterManager _npcMonsterManager;

    public MonsterSummoningModule(INpcMonsterManager npcMonsterManager, IMonsterEntityFactory entity, IGameLanguageService language, IAsyncEventPipeline eventPipeline)
    {
        _npcMonsterManager = npcMonsterManager;
        _entity = entity;
        _language = language;
        _eventPipeline = eventPipeline;
    }

    [Command("mob")]
    [Description("Spawn monster with given vnum")]
    public async Task<SaltyCommandResult> SpawnMob(
        [Description("Mob vnum")] short vnum,
        [Description("Amount")] short amount,
        [Description("Can move (false, true)")]
        bool canMove = false, bool isHostile = false)
    {
        IMonsterData monster = _npcMonsterManager.GetNpc(vnum);

        if (monster == null)
        {
            return new SaltyCommandResult(false, "The monster doesn't exist!");
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

        return new SaltyCommandResult(true, $"Spawned {amount}x of {_language.GetLanguage(GameDataType.NpcMonster, monster.Name, Context.Player.UserLanguage)}");
    }

    [Command("mobs")]
    [Description("Spawn monster with given vnum in a square")]
    public async Task<SaltyCommandResult> SpawnMobs(
        [Description("Mob vnum")] short vnum,
        [Description("Range")] byte range)
    {
        IClientSession session = Context.Player;
        IMonsterData monster = _npcMonsterManager.GetNpc(vnum);

        if (monster == null)
        {
            return new SaltyCommandResult(false, "The monster doesn't exist!");
        }

        var list = new List<ToSummon>();

        for (short y = 0; y < range; y++)
        {
            for (short x = 0; x < range; x++)
            {
                list.Add(new ToSummon
                {
                    VNum = vnum,
                    SpawnCell = new Position((short)(session.PlayerEntity.PositionX + x), (short)(session.PlayerEntity.PositionY + y)),
                    IsMoving = false,
                    IsHostile = true
                });
            }
        }

        await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(session.CurrentMapInstance, list));
        return new SaltyCommandResult(true, $"Spawned {list.Count}x of {_language.GetLanguage(GameDataType.NpcMonster, monster.Name, Context.Player.UserLanguage)}");
    }


    [RequireAuthority(AuthorityType.GA)]
    [Command("fill-mobs", "fill-mob", "fill-monsters", "fill-monster")]
    [Description("Spawn monster with given vnum")]
    public async Task<SaltyCommandResult> FillMapWithMonsters(
        [Description("Mob vnum")] ushort vnum,
        [Description("Amount")] short amount,
        [Description("Can move (false, true)")]
        bool canMove = false,
        [Description("IsAggressive (false, true)")]
        bool isAggresive = false)
    {
        IClientSession session = Context.Player;
        IMonsterData monster = _npcMonsterManager.GetNpc(vnum);

        if (monster == null)
        {
            return new SaltyCommandResult(false, "The monster doesn't exist!");
        }

        var list = new List<ToSummon>();

        for (int i = 0; i < amount; i++)
        {
            var toSummon = new ToSummon
            {
                VNum = vnum,
                SpawnCell = session.CurrentMapInstance.GetRandomPosition(),
                IsMoving = canMove,
                IsHostile = isAggresive
            };
            list.Add(toSummon);
        }

        await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(session.CurrentMapInstance, list));

        return new SaltyCommandResult(true, $"Spawned {amount}x of {_language.GetLanguage(GameDataType.NpcMonster, monster.Name, session.UserLanguage)}");
    }
}