using System;
using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Object.Common;

namespace WingsAPI.Scripting.Event.Raid
{
    [ScriptEvent("ThrowRaidDrops", true)]
    public class SThrowRaidDropsEvent : SEvent
    {
        public Guid BossId { get; set; }
        public IEnumerable<SDrop> Drops { get; set; }
        public byte DropsStackCount { get; set; }
        public SRange GoldRange { get; set; }
        public byte GoldStackCount { get; set; }
    }
}