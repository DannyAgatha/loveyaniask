using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Common.Map
{
    [ScriptObject]
    public class SMonsterWave
    {
        public short TimeInSeconds { get; set; }

        public IEnumerable<SMonster> Monsters { get; set; }

        public bool Loop { get; set; }

        public short? LoopTick { get; set; }
        
        public bool DisableShoutMessage { get; set; }

        public bool IsScaledWithPlayerAmount { get; set; }
    }
}