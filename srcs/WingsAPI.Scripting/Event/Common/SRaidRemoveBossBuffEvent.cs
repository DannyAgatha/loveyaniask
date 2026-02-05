using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.Common
{
    /// <summary>
    ///     Object representation of RemoveBossBuffEvent
    /// </summary>
    [ScriptEvent("RaidRemoveBossBuff", true)]
    public class SRaidRemoveBossBuffEvent : SEvent
    {
        /// <summary>
        ///     ID of the buff to remove
        /// </summary>
        public short Buff { get; set; }
    }
}