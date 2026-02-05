using System;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("TryStartTaskForMap", true)]
    public class STryStartTaskEvent : SEvent
    {
        public Guid MapId { get; set; }
    }
}