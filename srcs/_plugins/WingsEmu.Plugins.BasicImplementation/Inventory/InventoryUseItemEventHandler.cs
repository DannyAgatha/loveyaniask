using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryUseItemEventHandler : IAsyncEventProcessor<InventoryUseItemEvent>
{
    private readonly IItemHandlerContainer _itemHandler;

    public InventoryUseItemEventHandler(IItemHandlerContainer itemHandler) => _itemHandler = itemHandler;

    public async Task HandleAsync(InventoryUseItemEvent e, CancellationToken cancellation)
    {
        await Task.Run(() => _itemHandler.Handle(e.Sender, e), cancellation);
    }
}