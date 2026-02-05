using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common
{
    [ScriptObject]
    public class SDrop
    {
        public short ItemVnum { get; set; }
        public int Amount { get; set; }
    }

    [ScriptObject]
    public class SDropChance
    {
        public int Chance { get; set; }
        public short ItemVnum { get; set; }
        public int Amount { get; set; }
    }
}