using System;

namespace WingsEmu.Game.Act6.Configuration
{
    public class Act6Configuration
    {
        public double MaximumFactionPoints { get; set; } = 10000;

        public int FactionPointsPerPveKill { get; set; } = 1;

        public TimeSpan InstanceDuration { get; set; } = TimeSpan.FromHours(1);

        public TimeSpan PvpInstanceDuration { get; set; } = TimeSpan.FromMinutes(15);
    }
}