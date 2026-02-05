using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class RollGeneratedItemGuriHandler : IGuriHandler
{
    public long GuriEffectId => 4999;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.Data != 8023)
        {
            return;
        }

        if (guriPacket.User == null)
        {
            return;
        }

        short slot = (short)guriPacket.User.Value;
        InventoryItem box = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Main);
        if (box == null)
        {
            return;
        }

        await session.EmitEventAsync(new RollItemBoxEvent
        {
            Item = box
        });
    }
}