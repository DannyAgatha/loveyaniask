using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsEmu.DTOs.Inventory;

/// <summary>
///     Composite object
/// </summary>
[ProtoContract]
public class CharacterPartnerInventoryItemDto
{
    [ProtoMember(1)]
    public short PartnerSlot { get; set; }
    
    [ProtoMember(2)]
    public bool IsEquipped { get; set; }

    [ProtoMember(3)]
    public ItemInstanceDTO ItemInstance { get; set; }
}