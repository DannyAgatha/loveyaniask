using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum.TimeSpace;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("FinishTimeSpace", true)]
    public class STimeSpaceFinishEvent : SEvent
    {
        public STimeSpaceFinishType TimeSpaceFinishType { get; set; }
    }
}