using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class UseBoxGuriHandler : IGuriHandler
{
    public long GuriEffectId => 300;

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
        
        if (session.PlayerEntity.IsInExchange() || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsShopping || session.PlayerEntity.IsWarehouseOpen
            || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen || session.PlayerEntity.HasNosBazaarOpen)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"{session.CharacterName()} is trying to dupe boxes !");
            return;
        }

        short slot = (short)guriPacket.User.Value;
        InventoryItem box = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);

        if (box == null)
        {
            Log.Info("No box");
            return;
        }

        await session.EmitEventAsync(new RollItemBoxEvent
        {
            Item = box
        });
    }
}