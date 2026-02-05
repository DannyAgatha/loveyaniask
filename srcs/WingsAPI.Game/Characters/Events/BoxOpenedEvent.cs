using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class BoxOpenedEvent : PlayerEvent
{
    public ItemInstanceDTO Box { get; init; }
    public List<ItemInstanceDTO> Rewards { get; init; }
}