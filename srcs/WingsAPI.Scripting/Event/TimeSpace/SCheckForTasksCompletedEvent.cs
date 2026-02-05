using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("CheckForTasksCompleted", false)]
    public class SCheckForTasksCompletedEvent : SEvent
    {
        public IEnumerable<Guid> Maps { get; set; }
        public IEnumerable<SEvent> Events { get; set; }
    }
}