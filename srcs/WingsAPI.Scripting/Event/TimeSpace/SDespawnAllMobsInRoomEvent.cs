using System;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Event.TimeSpace
{
    [ScriptEvent("DespawnAllMobsInRoom", true)]
    public class SDespawnAllMobsInRoomEvent : SEvent
    {
        public Guid Map { get; set; }
    }
}