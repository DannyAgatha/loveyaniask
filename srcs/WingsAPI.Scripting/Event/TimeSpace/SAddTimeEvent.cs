using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("AddTime", true)]
    public class SAddTimeEvent : SEvent
    {
        public int Time { get; set; }
    }
}