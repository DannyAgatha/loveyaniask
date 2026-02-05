using System;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Configurations.Miniland;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameScoreLogEvent : PlayerEvent
{
    public long OwnerId { get; init; }
    public TimeSpan CompletionTime { get; init; }
    public int MinigameVnum { get; init; }
    public MinigameType MinigameType { get; init; }
    public long Score1 { get; init; }
    public long Score2 { get; init; }
}