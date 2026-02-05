using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common.Map;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("ClosePortal", true)]
    public class SClosePortal : SEvent
    {
        public SPortal Portal { get; set; }
    }
}