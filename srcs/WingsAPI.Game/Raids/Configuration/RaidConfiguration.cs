using System;

namespace WingsEmu.Game.Raids.Configuration;

public class RaidConfiguration
{
    public TimeSpan RaidMapDestroyDelay { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan RaidDeathRevivalDelay { get; set; } = TimeSpan.FromSeconds(20);

    public TimeSpan RaidSlowMoDelay { get; set; } = TimeSpan.FromSeconds(7);

    public int LivesPerCharacter { get; set; } = 3;
}