using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class PresentationMessageHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 203 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsOnVehicle || !session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (e.Option != 0)
        {
            return;
        }

        session.SendGuriPacket(10, 2, 1);
    }
}