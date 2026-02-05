using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common.Map
{
    [ScriptObject]
    public class SItem : SMapObject
    {
        public short Vnum { get; set; }
        public bool IsObjective { get; set; }
        public bool IsRandomPosition { get; set; }
        public bool IsRandomUniquePosition { get; set; }
        public int? DanceDuration { get; set; }
    }
}