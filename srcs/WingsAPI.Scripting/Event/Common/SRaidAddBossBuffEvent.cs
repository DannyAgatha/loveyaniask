using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum;
using WingsEmu.Game.Buffs;

namespace WingsAPI.Scripting.Event.Common
{
    /// <summary>
    ///     Object representation of RaidAddBossBuffEvent
    /// </summary>
    [ScriptEvent("RaidAddBossBuff", true)]
    public class SRaidAddBossBuffEvent : SEvent
    {
        /// <summary>
        ///     ID of the buff to add
        /// </summary>
        public short Buff { get; set; }
    }
}