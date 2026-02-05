using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Commands;

[Name("Act4_Commands")]
[Group("act4", "glacernon")]
[Description("Module related to Act4 management commands.")]
[RequireAuthority(AuthorityType.GA)]
public partial class Act4CommandsModule : SaltyModuleBase
{
    [Command("addFactionPoints", "addPoints", "afp")]
    public async Task AddFactionPoints(int points, FactionType factionType = FactionType.Neutral)
    {
        if (factionType == FactionType.Neutral)
        {
            await Context.Player.EmitEventAsync(new Act4FactionPointsIncreaseEvent(points));
        }

        await Context.Player.EmitEventAsync(new Act4FactionPointsIncreaseEvent(factionType, points));
    }
}