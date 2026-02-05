using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum;

namespace WingsAPI.Scripting.Event.Raid
{
    /// <summary>
    ///     Object representation of IncreaseObjectiveEvent
    /// </summary>
    [ScriptEvent("RaidIncreaseObjective", true)]
    public class SRaidIncreaseObjectiveEvent : SEvent
    {
        /// <summary>
        ///     Type of objective increased
        /// </summary>
        public SObjectiveType ObjectiveType { get; set; }
    }
}