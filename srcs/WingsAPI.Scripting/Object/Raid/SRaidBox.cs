using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Raid
{
    [ScriptObject]
    public class SRaidBox
    {
        public int RewardBox { get; set; }

        public IEnumerable<SRaidBoxRarity> RaidBoxRarity { get; set; }
    }
}