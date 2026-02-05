using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class DragonGemsItemHandler : IItemUsageByVnumHandler
{
    public long[] Vnums => new[]
    {
        (long)ItemVnums.FIRE_DRAGON_GEM, 
        (long)ItemVnums.ICE_DRAGON_GEM, 
        (long)ItemVnums.MOON_LIGHT_DRAGON_GEM, 
        (long)ItemVnums.SKY_DRAGON_GEM
    };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        string[] packet = e.Packet;
        InventoryItem item = e.Item;
        
        if (packet.Length < 10)
        {
            return;
        }

        if (!short.TryParse(packet[9], out short slot))
        {
            return;
        }

        if (!Enum.TryParse(packet[8], out InventoryType inventoryType) || inventoryType != InventoryType.Equipment)
        {
            return;
        }

        InventoryItem specialist = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (specialist == null)
        {
            return;
        }

        if (!specialist.ItemInstance.IsSpecialistCard())
        {
            return;
        }

        if (specialist.ItemInstance.Upgrade < 20)
        {
            session.SendInfoI(Game18NConstString.SpLevelTwentyEnchantedDragonItem); 
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.SpLevelTwentyEnchantedDragonItem);
            return;
        }

        byte newElement = (ItemVnums)item.ItemInstance.ItemVNum switch
        {
            ItemVnums.FIRE_DRAGON_GEM => (byte)ElementType.Fire,
            ItemVnums.ICE_DRAGON_GEM => (byte)ElementType.Water,
            ItemVnums.MOON_LIGHT_DRAGON_GEM => (byte)ElementType.Light,
            ItemVnums.SKY_DRAGON_GEM => (byte)ElementType.Shadow,
            _ => (byte)ElementType.Neutral
        };
        
        if (specialist.ItemInstance.SpGemElement == newElement)
        {
            session.SendInfoI(Game18NConstString.SpAlreadyHasDragonItem); 
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.SpAlreadyHasDragonItem);
            return;
        }
        
        specialist.ItemInstance.SpGemElement = newElement;
        
        session.SendMsgi(MessageType.Default, Game18NConstString.SpHasBeenEnchantedWithTheFollowingItem, 2, item.ItemInstance.ItemVNum);

        await session.RemoveItemFromInventory(item.ItemInstance.ItemVNum);
    }
}