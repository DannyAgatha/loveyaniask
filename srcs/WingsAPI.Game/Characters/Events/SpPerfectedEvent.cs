using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class SpPerfectedEvent : PlayerEvent
{
    public ItemInstanceDTO Sp { get; init; }
    public bool Success { get; init; }
    public byte SpPerfectionLevel { get; init; }
}