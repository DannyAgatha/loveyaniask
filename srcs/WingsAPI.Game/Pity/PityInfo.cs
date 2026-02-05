using System.Collections.Generic;
using WingsAPI.Packets.Enums;

namespace WingsEmu.Game.Pity;

public class PityInfo
{
    public PityType PityType { get; set; }
    public List<PityData> PityData { get; set; }
}