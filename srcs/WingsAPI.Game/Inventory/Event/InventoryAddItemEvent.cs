using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryAddItemEvent : PlayerEvent
{
    public InventoryAddItemEvent(InventoryItem inventoryItem, bool showMessage = false, ChatMessageColorType addItemMessageType = ChatMessageColorType.Green, bool sendAsGiftIfFull = false,
        MessageErrorType errorType = MessageErrorType.Chat, short? slot = null, InventoryType? inventoryType = null, bool isByMovePacket = false)
    {
        InventoryItem = inventoryItem;
        ShowMessage = showMessage;
        ItemMessageType = addItemMessageType;
        SendAsGiftIfFull = sendAsGiftIfFull;
        MessageErrorType = errorType;
        Slot = slot;
        InventoryType = inventoryType;
        IsByMovePacket = isByMovePacket;
    }

    public InventoryItem InventoryItem { get; }
    public bool ShowMessage { get; }
    public ChatMessageColorType ItemMessageType { get; }
    public bool SendAsGiftIfFull { get; }
    public MessageErrorType MessageErrorType { get; }
    public short? Slot { get; }
    public InventoryType? InventoryType { get; }
    public bool IsByMovePacket { get; }
}

public enum MessageErrorType
{
    Chat = 0,
    Shop = 1
}