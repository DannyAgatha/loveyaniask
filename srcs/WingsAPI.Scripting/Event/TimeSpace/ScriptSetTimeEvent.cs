using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("SetTime", true)]
    public class ScriptSetTimeEvent : SEvent
    {
        public int Time { get; set; }
    }
}