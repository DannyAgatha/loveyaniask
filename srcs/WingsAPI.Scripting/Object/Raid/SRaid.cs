using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum.Raid;
using WingsAPI.Scripting.Object.Common;

namespace WingsAPI.Scripting.Object.Raid
{
    /// <summary>
    ///     Object used to represent a raid in a script
    /// </summary>
    [ScriptObject]
    public class SRaid
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Type of raid
        /// </summary>
        public SRaidType RaidType { get; set; }

        /// <summary>
        ///     Requirements of the raid
        /// </summary>
        public SRaidRequirement Requirement { get; set; }

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

        public int DurationInSeconds { get; set; }
    }
}