using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Commands;

[Name("Admin-Raids")]
[Description("Module related to Raids Administrator commands.")]
[RequireAuthority(AuthorityType.GA)]
public class RaidAdminStartModule : SaltyModuleBase
{
    [Command("startRaid")]
    public async Task<SaltyCommandResult> StartRaidAsync()
    {
        RaidParty raidParty = Context.Player.PlayerEntity.Raid;
        Context.Player.EmitEvent(new RaidInstanceStartEvent());
        return new SaltyCommandResult(true);
    }

    [Command("createRaid")]
    public async Task<SaltyCommandResult> CreateRaid(byte raidType)
    {
        Context.Player.EmitEvent(new RaidPartyCreateEvent(raidType, null, false));
        return new SaltyCommandResult(true);
    }
}