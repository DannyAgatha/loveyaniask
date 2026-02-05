using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._ItemUsage;

public interface IItemHandlerContainer
{
    Task RegisterItemHandler(IItemHandler handler);
    Task RegisterItemHandler(IItemUsageByVnumHandler handler);

    Task UnregisterAsync(IItemHandler handler);
    Task UnregisterAsync(IItemUsageByVnumHandler handler);

    void Handle(IClientSession player, InventoryUseItemEvent e);

    Task HandleAsync(IClientSession player, InventoryUseItemEvent e);
}