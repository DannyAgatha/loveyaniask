using System;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.Enum.TimeSpace;

namespace WingsAPI.Scripting.Object.Common.Map
{
    /// <summary>
    ///     Object used to represent a portal in a script
    /// </summary>
    [ScriptObject]
    public class SPortal
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Id of the source map
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        ///     Id of the destination map
        /// </summary>
        public Guid DestinationId { get; set; }

        /// <summary>
        ///     Position of the portal in the source map
        /// </summary>
        public SPosition SourcePosition { get; set; }

        /// <summary>
        ///     Position where you're teleported in destination map
        /// </summary>
        public SPosition DestinationPosition { get; set; }

        public SPortalType Type { get; set; }

        public int? CreationDelay { get; set; }

        public bool IsReturn { get; set; }

        public SPortalMinimapOrientation PortalMiniMapOrientation { get; set; }
    }
}