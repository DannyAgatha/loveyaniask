// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.Mates;

[ProtoContract]
public class MateDTO : ILongDto
{
    [ProtoMember(2)]
    public byte Attack { get; set; }

    [ProtoMember(3)]
    public bool CanPickUp { get; set; }

    [ProtoMember(4)]
    public long CharacterId { get; set; }

    [ProtoMember(5)]
    public byte Defence { get; set; }

    [ProtoMember(6)]
    public byte Direction { get; set; }

    [ProtoMember(7)]
    public long Experience { get; set; }

    [ProtoMember(8)]
    public int Hp { get; set; }

    [ProtoMember(9)]
    public bool IsSummonable { get; set; }

    [ProtoMember(10)]
    public bool IsTeamMember { get; set; }

    [ProtoMember(11)]
    public byte Level { get; set; }

    [ProtoMember(12)]
    public short Loyalty { get; set; }

    [ProtoMember(13)]
    public short MapX { get; set; }

    [ProtoMember(14)]
    public short MapY { get; set; }

    [ProtoMember(15)]
    public MateType MateType { get; set; }

    [ProtoMember(16)]
    public int Mp { get; set; }

    [ProtoMember(17)]
    public string MateName { get; set; }

    [ProtoMember(18)]
    public int NpcMonsterVNum { get; set; }

    [ProtoMember(19)]
    public short Skin { get; set; }

    [ProtoMember(20)]
    public byte PetSlot { get; set; }

    [ProtoMember(21)]
    public short MinilandX { get; set; }

    [ProtoMember(22)]
    public short MinilandY { get; set; }

    [ProtoMember(23)]
    public bool IsLimited { get; set; }
    
    [ProtoMember(24)]
    public byte HeroLevel { get; set; }

    [ProtoMember(25)]
    public int TrainerExperience { get; set; }

    [ProtoMember(26)]
    public byte Stars { get; set; }
    
    [ProtoMember(27)]
    public List<int> TrainerSkills { get; set; } = new();
    
    [ProtoMember(28)]
    public bool HasDhaPremium { get; set; }
    
    [ProtoMember(29)]
    public bool IsDhaLootEnabled { get; set; }

    [ProtoMember(1)]
    public long Id { get; set; }
}