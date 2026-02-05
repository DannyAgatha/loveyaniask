// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsEmu.Game.Inventory;

public interface IItemUsageToggleManager
{
    Task<bool> IsItemBlocked(int vnum);

    Task BlockItemUsage(int vnum);
    Task UnblockItemUsage(int vnum);
    Task<IEnumerable<int>> GetBlockedItemUsages();
}