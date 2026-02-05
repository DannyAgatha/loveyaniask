using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class MateGuriHandler : IItemHandler
{
    public ItemType ItemType => ItemType.PetPartnerItem;

    public long[] Effects => new long[] { 13 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int x1))
        {
            return;
        }

        if (session.PlayerEntity.MateComponent.GetMate(x => x.Id == x1) == null)
        {
            return;
        }

        session.SendGuriPacket(10, 1, x1);
    }
}