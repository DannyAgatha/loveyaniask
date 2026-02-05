// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Buffs;

namespace WingsEmu.Game.Buffs;

public class Card : CardDTO
{
    public List<BCardDTO> BCards { get; set; }
}