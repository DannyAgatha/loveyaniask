// NosEmu
// 


using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Admin-RuneCheat")]
[Description("Module related to Administrator commands.")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorCheatModule_Rune : SaltyModuleBase
{
    [Command("addrune")]
    public async Task<SaltyCommandResult> AddRuneAsync(short slot, int bcardType, byte subType, byte value1, int value2) => new SaltyCommandResult(true);


    [Command("infoRune")]
    public async Task<SaltyCommandResult> DumpRuneInformation(short slot) => new SaltyCommandResult(true);


    [Command("clearrune")]
    public async Task<SaltyCommandResult> ClearRuneAsync(short slot) => new SaltyCommandResult(true);

    [Command("rmrune", "removerune")]
    public async Task<SaltyCommandResult> RemoveRuneAsync(short slot, int type) => new SaltyCommandResult(true);
}