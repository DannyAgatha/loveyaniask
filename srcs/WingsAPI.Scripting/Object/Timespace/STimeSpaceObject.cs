// NosEmu
// 


using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common;

namespace WingsAPI.Scripting.Object.Timespace
{
    [ScriptObject]
    public class ScriptTimeSpace
    {
        /// <summary>
        ///     Randomly generated id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        public int TimeSpaceId { get; set; }

        public STimeSpaceObjective Objectives { get; set; }

        /// <summary>
        ///     Maps of the timespace
        /// </summary>
        public IEnumerable<SMap> Maps { get; set; }

        /// <summary>
        ///     Spawn point of this timespace
        /// </summary>
        public SLocation Spawn { get; set; }

        /// <summary>
        ///     Duration of the timespace
        /// </summary>
        public int DurationInSeconds { get; set; }

        public byte Lives { get; set; }

        public int BonusPointItemDropChance { get; set; }

        public int? PreFinishDialog { get; set; }

        public bool PreFinishDialogIsObjective { get; set; }

        public short? ObtainablePartnerVnum { get; set; }

        public bool InfiniteDuration { get; set; }
    }
}