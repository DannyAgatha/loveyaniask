// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.DAL;
using WingsEmu.DTOs.BCards;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.Buffs;

public class CardDTO : IIntDto
{
    public int Duration { get; set; }

    public int EffectId { get; set; }

    public int GroupId { get; set; }

    public byte Level { get; set; }

    public string Name { get; set; }

    public short TimeoutBuff { get; set; }

    public int BuffType { get; set; }

    public byte TimeoutBuffChance { get; set; }

    public int SecondBCardsDelay { get; set; }

    public BuffCategory BuffCategory { get; set; }

    public byte BuffPartnerLevel { get; set; }

    public bool IsConstEffect { get; set; }

    public byte ElementType { get; set; }
    public List<BCardDTO> Bcards { get; set; } = new();
    public int Id { get; set; }
}