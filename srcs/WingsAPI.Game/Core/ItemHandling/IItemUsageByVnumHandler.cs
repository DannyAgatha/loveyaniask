using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._ItemUsage;

public interface IItemUsageByVnumHandler
{
    long[] Vnums { get; }

    Task HandleAsync(IClientSession session, InventoryUseItemEvent e);
}