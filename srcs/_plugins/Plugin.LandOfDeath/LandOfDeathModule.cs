using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.LandOfDeath.RecurrentJob;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.LandOfDeath.Events;

namespace Plugin.LandOfDeath;

[RequireAuthority(AuthorityType.GA)]
public class LandOfDeathModule : SaltyModuleBase
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public LandOfDeathModule(IAsyncEventPipeline asyncEventPipeline)
    {
        _asyncEventPipeline = asyncEventPipeline;
    }

    [Command("lod-start")]
    public async Task<SaltyCommandResult> LodStart()
    {
        await _asyncEventPipeline.ProcessEventAsync(new LandOfDeathStartEvent
        {
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow + TimeSpan.FromHours(2)
        });

        return new SaltyCommandResult(true);
    }
}
