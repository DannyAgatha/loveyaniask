using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common.Map
{
    [ScriptObject]
    public class SNpcWave
    {
        public short TimeInSeconds { get; set; }

        public IEnumerable<SMapNpc> Npcs { get; set; }

        public bool Loop { get; set; }

        public short? LoopTick { get; set; }

        public bool IsScaledWithPlayerAmount { get; set; }
    }
}