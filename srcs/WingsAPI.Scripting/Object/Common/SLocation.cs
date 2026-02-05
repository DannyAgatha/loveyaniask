using System;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common
{
    [ScriptObject]
    public class SLocation
    {
        public Guid MapId { get; set; }
        public SPosition Position { get; set; }
    }
}