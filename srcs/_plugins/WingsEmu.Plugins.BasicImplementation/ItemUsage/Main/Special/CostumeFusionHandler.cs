using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class CostumeFusionHandler : IItemUsageByVnumHandler
{
    public long[] Vnums => [(long)ItemVnums.COSTUME_FUSION, (long)ItemVnums.COSTUME_FUSION_LIMITED];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e) => session.SendGuriPacket(12, value: 79);
}