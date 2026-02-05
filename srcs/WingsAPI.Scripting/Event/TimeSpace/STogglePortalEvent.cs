using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common.Map;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("TogglePortal", false)]
    public class STogglePortalEvent : SEvent
    {
        public SPortal Portal { get; set; }
    }
}