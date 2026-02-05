using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common;

namespace WingsAPI.Scripting.Event.Common
{
    /// <summary>
    ///     Object represention of MonsterSummonEvent
    /// </summary>
    [ScriptEvent("MonsterSummon", true)]
    public class SMonsterSummonEvent : SEvent
    {
        /// <summary>
        ///     Monster who will be summoned
        /// </summary>
        public IEnumerable<SMonster> Monsters { get; set; }
    }
}