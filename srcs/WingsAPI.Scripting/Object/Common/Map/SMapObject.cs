using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Event;

namespace WingsAPI.Scripting.Object.Common.Map
{
    [ScriptObject]
    public class SMapObject
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Position of this object in the map
        /// </summary>
        public SPosition Position { get; set; }

        public IDictionary<string, IEnumerable<SEvent>> Events { get; set; }
    }
}