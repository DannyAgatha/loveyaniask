using System;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act6
{
    public class Act6InstanceObject
    {
        public Act6InstanceObject()
        {
        }

        public Act6InstanceObject(bool active, DateTime start, DateTime end, FactionType factionType = FactionType.Neutral)
        {
            InstanceActive = active;
            InstanceStart = start;
            InstanceEnd = end;
            InstanceFaction = factionType;
        }

        public bool InstanceActive { get; set; }

        public FactionType InstanceFaction { get; set; }

        public DateTime InstanceEnd { get; set; }

        public DateTime InstanceStart { get; set; }
    }
}