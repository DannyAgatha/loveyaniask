using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Act6.Event;

namespace Plugin.Act6.Commands
{
    [Name("Act6_Commands")]
    [Group("act6", "Cylloan")]
    [Description("Module related to Act6 management commands.")]
    [RequireAuthority(AuthorityType.DEV)]
    public partial class Act6CommandsModule : SaltyModuleBase
    {
        [Command("addAct6Points", "act6p")]
        public async Task AddFactionPoints(int points)
        {
            await Context.Player.EmitEventAsync(new Act6FactionPointsIncreaseEvent(points));
        }
    }
}
