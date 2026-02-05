using System;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class DailyRewardEvent : PlayerEvent
{
    public TimeSpan TimeSinceGameStart { get; set; }

    public ICharacterSystem CharacterSystem { get; set; }
}