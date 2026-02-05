using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Commands;

[Name("Admin-TimeSpaces")]
[Description("Module related to TimeSpaces Administrator commands.")]
[Group("ts", "timespace")]
[RequireAuthority(AuthorityType.GA)]
public class TimeSpaceAdminStartModule : SaltyModuleBase
{
    private readonly ITimeSpaceScriptManager _scriptManager;

    public TimeSpaceAdminStartModule(ITimeSpaceScriptManager scriptManager) => _scriptManager = scriptManager;

    [Command("start")]
    public async Task<SaltyCommandResult> StartTimeSpace(int timeSpaceId = 0)
    {
        Context.Player.EmitEvent(new TimeSpacePartyCreateEvent(timeSpaceId));
        if (!Context.Player.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return new SaltyCommandResult(false, "Failed");
        }

        Context.Player.EmitEvent(new TimeSpaceInstanceStartEvent());

        return new SaltyCommandResult(true, $"Started TimeSpace with Id {timeSpaceId}");
    }

    [Command("end")]
    public async Task<SaltyCommandResult> End(TimeSpaceFinishType type)
    {
        if (!Context.Player.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return new SaltyCommandResult(false, "Failed");
        }

        await Context.Player.EmitEventAsync(new TimeSpaceInstanceFinishEvent(Context.Player.PlayerEntity.TimeSpaceComponent.TimeSpace, type));
        return new SaltyCommandResult(true);
    }


    [Command("reload")]
    public async Task<SaltyCommandResult> ReloadScripts()
    {
        _scriptManager.Load();
        return new SaltyCommandResult(true, "Reloaded TimeSpace scripts");
    }
}