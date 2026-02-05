using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Raid
{
    [ScriptObject]
    public class SRaidBoxRarity
    {
        public byte Rarity { get; set; }
        public int Chance { get; set; }
    }
}