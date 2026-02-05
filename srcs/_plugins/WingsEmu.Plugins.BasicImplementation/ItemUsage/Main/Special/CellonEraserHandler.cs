// NosEmu
// 


using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class CellonEraserHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public CellonEraserHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 667 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!byte.TryParse(e.Packet[9], out byte iislot))
        {
            return;
        }
        InventoryItem wearInstance = session.PlayerEntity.GetItemBySlotAndType(iislot, InventoryType.Equipment);
        if (wearInstance == null || wearInstance.ItemInstance.GameItem.ItemType != ItemType.Jewelry || wearInstance.ItemInstance.EquipmentOptions.Count == 0)
        {
            return;
        }
        wearInstance.ItemInstance.EquipmentOptions.Clear();
        wearInstance.ItemInstance.Cellon = 0;
        await session.RemoveItemFromInventory(item: e.Item);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_ERASED, session.UserLanguage), MsgMessageType.Middle);
    }
}