using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common.Map;

namespace WingsAPI.Scripting.Event.Common
{
    [ScriptEvent("OpenPortal", true)]
    public class SOpenPortalEvent : SEvent
    {
        public SPortal Portal { get; set; }
    }
}