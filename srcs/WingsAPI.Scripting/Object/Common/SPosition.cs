using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common
{
    /// <summary>
    ///     Object used to represent a position in a script
    /// </summary>
    [ScriptObject]
    public class SPosition
    {
        /// <summary>
        ///     Position on X axis
        /// </summary>
        public short X { get; set; }

        /// <summary>
        ///     Position on Y axis
        /// </summary>
        public short Y { get; set; }
    }
}