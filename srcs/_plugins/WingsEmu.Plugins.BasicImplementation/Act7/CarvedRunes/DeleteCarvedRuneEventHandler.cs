using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act7.CarvedRunes;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act7.CarvedRunes;

public class DeleteCarvedEventHandler : IAsyncEventProcessor<RemoveCarvedRuneEvent>
{
    public async Task HandleAsync(RemoveCarvedRuneEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance item = e.Equipment.ItemInstance;

        if (!session.PlayerEntity.HasItem(e.Equipment.ItemInstance.ItemVNum))
        {
            // PacketLogger
            return;
        }

        if (item.GameItem.EquipmentSlot != EquipmentType.MainWeapon && item.GameItem.EquipmentSlot != EquipmentType.Armor)
        {
            // Cannot upgrade if it's not MainWeapon or Armor.
            return;
        }

        if (item.CarvedRunes.BCards == null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.WeaponHaveNoRunes);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.RUNE_REMOVAL_HAMMER))
        {
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        item.CarvedRunes = new CarvedRunesDto();
        await session.RemoveItemFromInventory((short)ItemVnums.RUNE_REMOVAL_HAMMER);
        session.SendMsgi(MessageType.Default, Game18NConstString.RuneRemoved);
        session.SendInventoryAddPacket(e.Equipment);
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}
