using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class ShellIdentifiedEvent : PlayerEvent
{
    public ItemInstanceDTO Shell { get; init; }
}