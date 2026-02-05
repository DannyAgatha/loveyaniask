using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Event;

namespace WingsAPI.Scripting.Object.Common
{
    [ScriptObject]
    public class SMapNpc
    {
        public Guid Id { get; set; }

        public short Vnum { get; set; }

        public SPosition Position { get; set; }

        public bool CanMove { get; set; }

        public bool IsProtectedNpc { get; set; }

        public bool FollowPlayer { get; set; }

        public byte Direction { get; set; }

        public float? HpMultiplier { get; set; }

        public float? MpMultiplier { get; set; }

        public byte? CustomLevel { get; set; }

        public IDictionary<string, IEnumerable<SEvent>> Events { get; set; }
    }
}