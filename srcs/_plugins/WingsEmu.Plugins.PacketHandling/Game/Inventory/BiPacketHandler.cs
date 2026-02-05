using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class BiPacketHandler : GenericGamePacketHandlerBase<BiPacket>
{
    private readonly IGameLanguageService _language;

    public BiPacketHandler(IGameLanguageService language) => _language = language;

    protected override async Task HandlePacketAsync(IClientSession session, BiPacket packet)
    {
        InventoryItem itemToRemove = session.PlayerEntity.GetItemBySlotAndType(packet.Slot, packet.InventoryType);
        if (itemToRemove == null)
        {
            return;
        }

        switch (packet.Option)
        {
            case null:
                session.SendDialog($"b_i {(byte)packet.InventoryType} {packet.Slot} 1", "b_i 0 0 5", _language.GetLanguage(GameDialogKey.INVENTORY_DIALOG_ASK_TO_DELETE, session.UserLanguage));

                break;
            case 1:
                session.SendDialog($"b_i {(byte)packet.InventoryType} {packet.Slot} 2", $"b_i {(byte)packet.InventoryType} {packet.Slot} 5",
                    _language.GetLanguage(GameDialogKey.INVENTORY_DIALOG_SURE_TO_DELETE, session.UserLanguage));

                break;
            case 2:
                if (session.PlayerEntity.IsInExchange())
                {
                    return;
                }

                if (session.PlayerEntity.HasShopOpened)
                {
                    return;
                }

                await session.RemoveItemFromInventory(amount: (short)itemToRemove.ItemInstance.Amount, item: itemToRemove);
                break;
        }
    }
}