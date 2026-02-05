using PhoenixLib.Events;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event;

public sealed record Act4DungeonSystemStartEvent(FactionType FactionType, DungeonType? DungeonType = null) : IAsyncEvent;