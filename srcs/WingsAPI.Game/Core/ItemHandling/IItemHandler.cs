using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game._ItemUsage;

public interface IItemHandler
{
    ItemType ItemType { get; }

    long[] Effects { get; }

    Task HandleAsync(IClientSession session, InventoryUseItemEvent e);
}