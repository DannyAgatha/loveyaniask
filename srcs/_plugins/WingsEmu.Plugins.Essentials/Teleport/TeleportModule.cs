using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.Teleport;

[Name("Teleport")]
[Description("Module related to teleport commands.")]
[RequireAuthority(AuthorityType.GM)]
public class TeleportModule : SaltyModuleBase
{
    private readonly IGameLanguageService _language;
    private readonly IMapManager _mapManager;

    public TeleportModule(IGameLanguageService languageService, IMapManager mapManager)
    {
        _mapManager = mapManager;
        _language = languageService;
    }

    [Command("tp", "go")]
    [Description("Teleport yourself to given MapId.")]
    public async Task<SaltyCommandResult> TeleportAsync(
        [Description("MapId.")] short MapId)
    {
        IClientSession session = Context.Player;
        await _mapManager.TeleportOnRandomPlaceInMapAsync(session, _mapManager.GetBaseMapInstanceByMapId(MapId));
        return new SaltyCommandResult(true);
    }

    [Command("tp", "go")]
    [Description("Teleport yourself to given x and y on current map.")]
    public async Task<SaltyCommandResult> TeleportAsync(
        [Description("Coordinates: X and y")] short x, short y)
    {
        IClientSession session = Context.Player;
        session.PlayerEntity.TeleportOnMap(x, y, true);
        return new SaltyCommandResult(true);
    }

    [Command("tp", "go")]
    [Description("Teleport yourself to given MapId on x and y.")]
    public async Task<SaltyCommandResult> TeleportAsync(
        [Description("MapId, x and y.")] short MapId, short x, short y)
    {
        IClientSession session = Context.Player;
        session.ChangeMap(MapId, x, y);
        return new SaltyCommandResult(true);
    }

    [Command("goto")]
    [Description("Teleport yourself to given player.")]
    public async Task<SaltyCommandResult> GotoAsync(
        [Description("Player's name.")] IClientSession target)
    {
        IClientSession session = Context.Player;
        session.ChangeMap(target.CurrentMapInstance, target.PlayerEntity.PositionX, target.PlayerEntity.PositionY);
        return new SaltyCommandResult(true);
    }

    [Command("summon")]
    [Description("Teleport given player to yourself.")]
    public async Task<SaltyCommandResult> SummonAsync(
        [Description("Player's name.")] IClientSession target)
    {
        IClientSession session = Context.Player;
        target.ChangeMap(session.CurrentMapInstance, session.PlayerEntity.PositionX, session.PlayerEntity.PositionY);
        return new SaltyCommandResult(true);
    }

    [Command("act4")]
    [Description("Brings you to act4")]
    public async Task<SaltyCommandResult> GotoAct4Async()
    {
        await Context.Player.EmitEventAsync(new PlayerChangeChannelAct4Event());
        return new SaltyCommandResult(true);
    }

    [Command("act4leave")]
    [Description("Brings you to alveus")]
    public async Task<SaltyCommandResult> LeaveoAct4Async()
    {
        await Context.Player.EmitEventAsync(new PlayerReturnFromAct4Event());
        return new SaltyCommandResult(true);
    }

    [Command("mfield")]
    [Description("Teleport yourself to your Mini-Land.")]
    public async Task<SaltyCommandResult> MfieldAsync()
    {
        IClientSession session = Context.Player;

        session.ChangeMap(session.PlayerEntity.Miniland);
        return new SaltyCommandResult(true);
    }

    [Command("mjoin")]
    [Description("Teleport yourself to player's Mini-Land.")]
    public async Task<SaltyCommandResult> MjoinAsync(
        [Description("Player's name.")] IClientSession target)
    {
        IClientSession session = Context.Player;
        session.ChangeMap(target.PlayerEntity.Miniland);
        return new SaltyCommandResult(true);
    }
}