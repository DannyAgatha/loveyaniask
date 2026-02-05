using System;

namespace WingsEmu.Game.Act4.Configuration
{
    public class Act4CaligorConfiguration
    {
        public TimeSpan CaligorDuration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan FernonDurationRaidInstance { get; set; } = TimeSpan.FromMinutes(180);
    }
}