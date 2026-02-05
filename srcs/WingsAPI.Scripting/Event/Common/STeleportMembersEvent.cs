using System;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common;

namespace WingsAPI.Scripting.Event.Common
{
    [ScriptEvent("Teleport", false)]
    public class STeleportMembersEvent : SEvent
    {
        public Guid MapInstanceId { get; set; }
        public SPosition SourcePosition { get; set; }
        public SPosition DestinationPosition { get; set; }
        public byte Range { get; set; }
    }
}