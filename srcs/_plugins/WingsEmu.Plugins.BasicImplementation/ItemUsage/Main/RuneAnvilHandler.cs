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

public class RuneAnvilHandler : IItemUsageByVnumHandler
{
    public long[] Vnums => new[] { (long)ItemVnums.REPAIR_RUNE_ANVIL };
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
        
        if (!short.TryParse(e.Packet[9], out short eqSlot1) ||
            !Enum.TryParse(e.Packet[8], out InventoryType eqType1))
        {
            return;
        }
        
        InventoryItem eq1 = session.PlayerEntity.GetItemBySlotAndType(eqSlot1, eqType1);
        
        if (eq1 == null)
        {
            // PACKET MODIFIED
            return;
        }
        
        if (eq1.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }
        GameItemInstance eqItem1 = eq1.ItemInstance;
        
        if (eqItem1.GameItem.ItemType != ItemType.Armor && eqItem1.GameItem.ItemType != ItemType.Weapon)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.OnlyUseOnDamagedWeapon);
            return;
        }
        
        if (!eqItem1.CarvedRunes.IsDamaged)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.WeaponIsntDamaged);
            return;
        }
        
        eqItem1.CarvedRunes.IsDamaged = false;
        session.SendMsgi(MessageType.Default, Game18NConstString.FixedDamagedWeapon);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}