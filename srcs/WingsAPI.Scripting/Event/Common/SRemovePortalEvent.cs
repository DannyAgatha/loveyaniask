using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common.Map;

namespace WingsAPI.Scripting.Event.Common
{
    [ScriptEvent(Name, true)]
    public class SRemovePortalEvent : SEvent
    {
        public const string Name = "RemovePortal";
        public SPortal Portal { get; set; }
    }
}