using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum.Raid;

namespace WingsAPI.Scripting.Event.Raid
{
    [ScriptEvent("FinishRaid", true)]
    public class SFinishRaidEvent : SEvent
    {
        public SRaidFinishType FinishType { get; set; }
    }
}