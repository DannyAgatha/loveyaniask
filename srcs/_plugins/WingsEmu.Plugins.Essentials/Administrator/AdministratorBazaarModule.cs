// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Communication.Bazaar;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Admin-BazaarModule")]
[Description("Module related to Administrator commands for Bazaar.")]
[Group("bazaar")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorBazaarModule : SaltyModuleBase
{
    private readonly IBazaarService _bazaarService;

    public AdministratorBazaarModule(IBazaarService bazaarService) => _bazaarService = bazaarService;

    [Command("unlist-vnum", "unlist", "remove-vnum", "remove")]
    public async Task<SaltyCommandResult> UnlistItemsFromBazaar([Remainder] string toUnlist)
    {
        string[] rawVnums = toUnlist.Split(' ');
        var vnums = new List<int>();

        foreach (string vnum in rawVnums)
        {
            if (!int.TryParse(vnum, out int parsedVnum))
            {
                return new SaltyCommandResult(false, $"Incorrect value: {vnum} is not an integer");
            }

            vnums.Add(parsedVnum);
        }

        UnlistItemFromBazaarResponse tmp = await _bazaarService.UnlistItemsFromBazaarWithVnumAsync(new UnlistItemFromBazaarRequest
        {
            Vnum = vnums
        });
        return new SaltyCommandResult(true, $"{tmp.UnlistedItems} items unlisted");
    }

    [Command("unlist-char", "remove-char")]
    public async Task<SaltyCommandResult> UnlistItemsFromBazaarByCharacterId(int characterId)
    {
        UnlistItemFromBazaarResponse tmp = await _bazaarService.UnlistCharacterItemsFromBazaarAsync(new UnlistCharacterItemsFromBazaarRequest
        {
            Id = characterId
        });
        return new SaltyCommandResult(true, $"{tmp.UnlistedItems} items unlisted");
    }
}