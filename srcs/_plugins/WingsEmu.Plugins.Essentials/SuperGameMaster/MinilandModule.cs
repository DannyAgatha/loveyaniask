using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland.Minigames;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Miniland")]
[Description("Module related to Miniland commands.")]
[Group("miniland", "minigame")]
[RequireAuthority(AuthorityType.SGM)]
public class MinilandModule : SaltyModuleBase
{
    private readonly MinigameConfiguration _minigameConfiguration;

    public MinilandModule(MinigameConfiguration minigameConfiguration) => _minigameConfiguration = minigameConfiguration;


    [Command("refresh-p", "refresh-points", "rp")]
    [Description("Refresh your daily minigame points.")]
    public async Task<SaltyCommandResult> MinigameRefreshPoints([Description("Force refresh")] bool force = false)
    {
        Context.Player.EmitEvent(new MinigameRefreshProductionEvent(force));
        return new SaltyCommandResult(true);
    }

    [Command("points", "mgp")]
    [Description("Sets your minigame points.")]
    public async Task<SaltyCommandResult> MinigamePoints([Description("Minigame Points to set")] short minigamePoints)
    {
        if (minigamePoints > _minigameConfiguration.Configuration.MaxmimumMinigamePoints || minigamePoints < 0)
        {
            return new SaltyCommandResult(false);
        }

        Context.Player.PlayerEntity.MinilandPoint = minigamePoints;
        return new SaltyCommandResult(true);
    }

    [Command("points", "mgp")]
    [Description("Sets your minigame points.")]
    public async Task<SaltyCommandResult> MinigamePoints(IClientSession target, [Description("Minigame Points to set")] short minigamePoints)
    {
        if (minigamePoints > _minigameConfiguration.Configuration.MaxmimumMinigamePoints || minigamePoints < 0)
        {
            return new SaltyCommandResult(false);
        }

        target.PlayerEntity.MinilandPoint = minigamePoints;
        return new SaltyCommandResult(true);
    }
}