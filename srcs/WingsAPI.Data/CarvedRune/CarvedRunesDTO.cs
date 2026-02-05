// NosEmu
// 
// Developed by NosWings Team

using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.BCards;

namespace WingsAPI.Data.CarvedRune;

[ProtoContract]
public class CarvedRunesDto
{
    [ProtoMember(1)]
    public byte Upgrade { get; set; }

    [ProtoMember(2)]
    public bool IsDamaged { get; set; }

    [ProtoMember(3)]
    public List<BCardDTO> BCards { get; set; }
    
    [ProtoMember(4)]
    public bool CanUseRuneSolvent { get; set; }
}