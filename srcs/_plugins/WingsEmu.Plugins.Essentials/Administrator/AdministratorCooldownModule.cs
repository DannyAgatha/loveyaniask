using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Families;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Admin-Cooldown-Module")]
[Description("Module related to Administrator commands for hours/days cooldowns")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorCooldownModule : SaltyModuleBase
{
    private readonly IFamilyManager _familyManager;

    public AdministratorCooldownModule(IFamilyManager familyManager) => _familyManager = familyManager;

    [Command("family-cooldown")]
    [Description("Remove family join cooldown")]
    public async Task<SaltyCommandResult> FamilyCooldownAsync(IClientSession target)
    {
        if (!await _familyManager.RemovePlayerJoinCooldownAsync(target.PlayerEntity.Id))
        {
            return new SaltyCommandResult(false, "Player didn't had family any cooldown.");
        }

        return new SaltyCommandResult(true, $"Family join cooldown has been removed from {target.PlayerEntity.Name}");
    }
}