// NosEmu
// 


using System;
using ProtoBuf;

namespace WingsAPI.Data.Miniland;

[ProtoContract]
public class CharacterMinilandObjectDto
{
    [ProtoMember(1)]
    public Guid Id { get; set; }

    [ProtoMember(2)]
    public short InventorySlot { get; set; }

    [ProtoMember(3)]
    public byte Level1BoxAmount { get; set; }

    [ProtoMember(4)]
    public byte Level2BoxAmount { get; set; }

    [ProtoMember(5)]
    public byte Level3BoxAmount { get; set; }

    [ProtoMember(6)]
    public byte Level4BoxAmount { get; set; }

    [ProtoMember(7)]
    public byte Level5BoxAmount { get; set; }

    [ProtoMember(8)]
    public short MapX { get; set; }

    [ProtoMember(9)]
    public short MapY { get; set; }
}