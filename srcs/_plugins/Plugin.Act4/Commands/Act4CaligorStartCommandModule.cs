using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Act4.Event;

namespace Plugin.Act4.Commands;

[Group("act4", "glacernon")]
[Description("Module related to Act4 management commands.")]
[RequireAuthority(AuthorityType.GA)]
public partial class Act4CaligorStartCommandModule : SaltyModuleBase
{
    [Command("startCaligor", "sc")]
    public async Task StartCaligorEvent()
    {
        await Context.Player.EmitEventAsync(new Act4CaligorStartEvent());
    }
    
    [Command("endCaligor", "ec")]
    public async Task EndCaligorEvent()
    {
        await Context.Player.EmitEventAsync(new Act4CaligorEndEvent());
    }
}
