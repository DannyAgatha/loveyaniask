// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Administrator.Items;

[Group("itemtoggle", "itoggle", "itog")]
[Name("Admin-ItemManagement")]
[Description("Module related to Administrator commands for management.")]
[RequireAuthority(AuthorityType.Owner)]
public class AdministratorItemManagementModule : SaltyModuleBase
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly IItemUsageToggleManager _itemUsageToggleManager;

    public AdministratorItemManagementModule(IItemUsageToggleManager itemUsageToggleManager, IItemsManager itemsManager, IGameLanguageService gameLanguage)
    {
        _itemUsageToggleManager = itemUsageToggleManager;
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
    }

    [Command("block", "restrict", "disable", "d", "r")]
    public async Task<SaltyCommandResult> BlockItemUsage(int vnum)
    {
        await _itemUsageToggleManager.BlockItemUsage(vnum);
        return new SaltyCommandResult(true, $"{vnum} has been temporarily blocked!");
    }

    [Command("unblock", "authorize", "enable", "e", "a")]
    public async Task<SaltyCommandResult> UnblockItemUsage(int vnum)
    {
        await _itemUsageToggleManager.UnblockItemUsage(vnum);
        return new SaltyCommandResult(true, $"{vnum} has been unblocked!");
    }

    [Command("list", "get")]
    public async Task<SaltyCommandResult> ListCurrentlyBlockedItemUsage()
    {
        IEnumerable<int> tmp = await _itemUsageToggleManager.GetBlockedItemUsages();
        Context.Player.SendChatMessage("[BLOCKED_ITEMS]", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        foreach (int itemBlocked in tmp)
        {
            IGameItem itemName = _itemsManager.GetItem(itemBlocked);
            Context.Player.SendChatMessage($"[{itemBlocked}] => {_gameLanguage.GetLanguage(GameDataType.Item, itemName.Name, Context.Player.UserLanguage)}", ChatMessageColorType.Green);
        }

        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }
}