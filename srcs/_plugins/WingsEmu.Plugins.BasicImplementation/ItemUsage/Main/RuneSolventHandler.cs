using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class RuneSolventHandler : IItemUsageByVnumHandler
{
    public long[] Vnums => new[] { (long)ItemVnums.RUNE_SOLVENT };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet == null)
        {
            return;
        }

        if (e.Packet.Length < 9)
        {
            // MODIFIED PACKET
            return;
        }

        if (!short.TryParse(e.Packet[9], out short eqSlot2) ||
            !Enum.TryParse(e.Packet[8], out InventoryType eqType2))
        {
            return;
        }

        InventoryItem eq2 = session.PlayerEntity.GetItemBySlotAndType(eqSlot2, eqType2);
        if (eq2 == null)
        {
            // PACKET MODIFIED
            return;
        }

        if (eq2.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.RuneSolventCannotBeUsed);
            return;
        }

        GameItemInstance eqItem2 = eq2.ItemInstance;

        if (eqItem2.GameItem.ItemType != ItemType.Armor && eqItem2.GameItem.ItemType != ItemType.Weapon)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.RuneSolventCannotBeUsed);
            return;
        }

        if (eqItem2.CarvedRunes == null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.RuneSolventCannotBeUsed);
            return;
        }

        if (!eqItem2.CarvedRunes.CanUseRuneSolvent)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.RuneSolventCannotBeUsed);
            return;
        }

        eqItem2.CarvedRunes.CanUseRuneSolvent = false;
        eqItem2.CarvedRunes.Upgrade--;
        eqItem2.CarvedRunes.BCards.RemoveAt(eqItem2.CarvedRunes.BCards.Count - 1);
        session.SendMsgi(MessageType.Default, Game18NConstString.RuneLevelReduced);
        await session.RemoveItemFromInventory((short)ItemVnums.RUNE_SOLVENT);
        session.SendInventoryAddPacket(eq2);
    }
}