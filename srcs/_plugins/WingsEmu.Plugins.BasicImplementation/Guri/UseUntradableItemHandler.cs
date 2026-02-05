using PhoenixLib.MultiLanguage;
using System;
using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class UseMagicSpeedBoosterHandler : IGuriHandler
{
    public long GuriEffectId => 305;
    private readonly IItemsManager _itemsManager;

    public UseMagicSpeedBoosterHandler(IItemsManager itemsManager)
    {
        _itemsManager = itemsManager;
    }

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {

        IGameItem item = _itemsManager.GetItem(e.Data);
        if (item == null || item.ItemSubType != 7)
        {
            return;
        }


        short slot = (short)e.User.Value;
        InventoryItem invItem = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (invItem == null)
        {
            return;
        }

        invItem.ItemInstance.BoundCharacterId = session.PlayerEntity.Id;

        invItem.ItemInstance.ItemDeleteTime = invItem.ItemInstance.GameItem.LevelMinimum switch
        {
            0 => null,
            24 => DateTime.UtcNow.AddDays(1),
            72 => DateTime.UtcNow.AddDays(3),
            168 => DateTime.UtcNow.AddDays(7),
            _ => invItem.ItemInstance.ItemDeleteTime
        };

        session.SendInventoryAddPacket(invItem);
        await session.EmitEventAsync(new SpeedBoosterEvent());
    }
}
