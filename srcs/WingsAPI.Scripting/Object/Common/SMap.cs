using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.Object.Common.Map;
using WingsAPI.Scripting.Object.Timespace;

namespace WingsAPI.Scripting.Object.Common
{
    /// <summary>
    ///     Object used to represent a map in a script
    /// </summary>
    [ScriptObject]
    public sealed class SMap
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        public SMapType MapType { get; set; }

        /// <summary>
        ///     Vnum or Id of the map
        /// </summary>
        public int MapIdVnum { get; set; }

        public int NameId { get; set; }

        public int MusicId { get; set; }

        /// <summary>
        ///     Used for TimeSpace minimap
        /// </summary>
        public byte MapIndexX { get; set; }

        /// <summary>
        ///     Used for TimeSpace minimap
        /// </summary>
        public byte MapIndexY { get; set; }
        
        /// <summary>
        ///     Used for teleporting all the raid members on the BossRoom
        /// </summary>
        public bool IsBossRoom { get; set; }

        /// <summary>
        ///     Contains all the flags needed to define a map via VNum
        /// </summary>
        public IEnumerable<SMapFlags> Flags { get; set; }

        /// <summary>
        ///     Contains all monsters who need to be spawned in this map
        /// </summary>
        public IEnumerable<SMonster> Monsters { get; set; }

        public IEnumerable<SMapNpc> Npcs { get; set; }

        /// <summary>
        ///     Contains all buttons who need to be added to this map
        /// </summary>
        public IEnumerable<SMapObject> Objects { get; set; }

        /// <summary>
        ///     Contains all portals who need to be added to this map
        /// </summary>
        public IEnumerable<SPortal> Portals { get; set; }

        public IDictionary<string, IEnumerable<SEvent>> Events { get; set; }

        /// <summary>
        ///     Spawn x monsters every y seconds
        /// </summary>
        public IEnumerable<SMonsterWave> MonsterWaves { get; set; }
        
        
        /// <summary>
        ///     Spawn x npcs every y seconds
        /// </summary>
        public IEnumerable<SNpcWave> NpcWaves { get; set; }

        public STimeSpaceTask TimeSpaceTask { get; set; }
    }
}