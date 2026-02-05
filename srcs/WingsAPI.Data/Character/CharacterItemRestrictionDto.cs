using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class CharacterItemRestrictionDto
{
    [ProtoMember(1)]
    public List<ItemRestrictionDTO> Items { get; set; }
}