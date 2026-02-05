using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common.Map
{
    /// <summary>
    ///     Object used to represent a button in a script
    /// </summary>
    [ScriptObject]
    public class SButton : SMapObject
    {
        /// <summary>
        ///     Vnum of button when it's activated
        /// </summary>
        public short ActivatedVnum { get; set; }

        /// <summary>
        ///     Vnum of button when it's deactivated
        /// </summary>
        public short DeactivatedVnum { get; set; }

        public bool IsObjective { get; set; }

        public bool IsRandomPosition { get; set; }

        public bool OnlyOnce { get; set; }

        public int? CustomDanceDuration { get; set; }
    }
}