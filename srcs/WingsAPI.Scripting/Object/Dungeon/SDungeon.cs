using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum.Dungeon;
using WingsAPI.Scripting.Object.Common;
using WingsAPI.Scripting.Object.Raid;

namespace WingsAPI.Scripting.Object.Dungeon
{
    [ScriptObject]
    public class SDungeon
    {
        /// <summary>
        ///     The type of Dungeon
        /// </summary>
        public SDungeonType DungeonType { get; set; }

        /// <summary>
        ///     Maps in this raid
        /// </summary>
        public IEnumerable<SMap> Maps { get; set; }

        /// <summary>
        ///     Spawn point of this raid
        /// </summary>
        public SLocation Spawn { get; set; }

        /// <summary>
        ///     Raid Rewards
        /// </summary>
        public SRaidReward Reward { get; set; }
    }
}