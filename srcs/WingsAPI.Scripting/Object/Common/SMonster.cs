using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Object.Raid;

namespace WingsAPI.Scripting.Object.Common
{
    /// <summary>
    ///     Object used to represent a monster in a script
    /// </summary>
    [ScriptObject]
    public class SMonster
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Vnum of monster
        /// </summary>
        public short Vnum { get; set; }

        /// <summary>
        ///     Position where this monster will be spawned
        /// </summary>
        public SPosition Position { get; set; }

        public bool IsRandomPosition { get; set; }

        /// <summary>
        ///     Define if monster is a boss or not
        /// </summary>
        public bool IsBoss { get; set; }

        /// <summary>
        ///     Define if monster can move or not
        /// </summary>
        public bool CanMove { get; set; }

        /// <summary>
        ///     Define if monster is a target
        /// </summary>
        public bool IsTarget { get; set; }

        /// <summary>
        ///     Define if monster should walk to boss position
        /// </summary>
        public SPosition GoToBossPosition { get; set; }

        public IEnumerable<SDropChance> Drop { get; set; }

        public IDictionary<string, IEnumerable<SEvent>> Events { get; set; }
        
        public bool IgnoreSkillRange { get; set; }
        
        public bool SpawnAfterTask { get; set; }

        public int SpawnAfterMobs { get; set; }

        public byte Direction { get; set; }

        public byte? CustomLevel { get; set; }

        public float? HpMultiplier { get; set; }

        public float? MpMultiplier { get; set; }

        public string AtAroundMobId { get; set; }

        public byte? AtAroundMobRange { get; set; }

        public IEnumerable<SWaypoint> Waypoints { get; set; }
    }
}