// NosEmu
// 


using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("SpecialistModule")]
[Description("Module related to specialist commands.")]
[RequireAuthority(AuthorityType.GM)]
[Group("sp", "specialist")]
public class SpecialistModule : SaltyModuleBase
{
    [Command("perf", "perfection")]
    [RequireAuthority(AuthorityType.GA)]
    public async Task<SaltyCommandResult> PerfectSpecialist(IClientSession session, byte atk, byte def, byte elem, byte hpmp, byte fireRes, byte waterRes, byte lightRes, byte darkRes, byte spStoneUpgrade)
    {
        IPlayerEntity targetEntity = session.PlayerEntity;

        if (targetEntity.Specialist == null)
        {
            return new SaltyCommandResult(false, "SP not equipped");
        }

        GameItemInstance specialist = targetEntity.Specialist;

        specialist.SpDamage = atk;
        specialist.SpDefence = def;
        specialist.SpElement = elem;
        specialist.SpHP = hpmp;
        specialist.SpFire = fireRes;
        specialist.SpWater = waterRes;
        specialist.SpLight = lightRes;
        specialist.SpDark = darkRes;
        specialist.SpStoneUpgrade = spStoneUpgrade;

        targetEntity.SpecialistComponent.RefreshSlStats(session.PlayerEntity.Specialist.CurrentActiveSpecialistSlot);

        return new SaltyCommandResult(true, "SP perfection updated");
    }

    [Command("perf", "perfection")]
    public async Task<SaltyCommandResult> PerfectSpecialist(byte atk, byte def, byte elem, byte hpmp, byte fireRes, byte waterRes, byte lightRes, byte darkRes, byte spStoneUpgrade) =>
        await PerfectSpecialist(Context.Player, atk, def, elem, hpmp, fireRes, waterRes, lightRes, darkRes, spStoneUpgrade);
}