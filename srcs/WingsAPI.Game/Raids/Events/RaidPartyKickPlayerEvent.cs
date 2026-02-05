using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyKickPlayerEvent : PlayerEvent
{
    public RaidPartyKickPlayerEvent(long characterId) => CharacterId = characterId;

    public long CharacterId { get; }
}