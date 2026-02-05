using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Raid
{
    [ScriptObject]
    public class SWaypoint
    {
        public short X { get; set; }
        public short Y { get; set; }
        public int WaitTime { get; set; }
    }
}