// NosEmu
// 


using ProtoBuf;
using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.Inventory;

/// <summary>
///     Composite object
///     Character + Slot are unique
/// </summary>
[ProtoContract]
public class CharacterInventoryItemDto
{
    [ProtoMember(1)]
    public long CharacterId { get; set; }

    [ProtoMember(2)]
    public short Slot { get; set; }

    [ProtoMember(3)]
    public InventoryType InventoryType { get; set; }

    [ProtoMember(4)]
    public bool IsEquipped { get; set; }

    [ProtoMember(5)]
    public ItemInstanceDTO ItemInstance { get; set; }
}