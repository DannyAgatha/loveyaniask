using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("RemoveItems", true)]
    public class SRemoveItemsEvent : SEvent
    {
        public IEnumerable<Guid> Items { get; set; }
    }
}