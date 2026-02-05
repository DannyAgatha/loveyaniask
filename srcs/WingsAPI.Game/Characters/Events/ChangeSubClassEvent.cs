using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters.Events;

public class ChangeSubClassEvent : PlayerEvent
{
    public SubClassType NewSubClass { get; set; }
    public bool ShouldObtainBasicItems { get; set; }
    public byte TierLevel { get; set; }
    public long TierExperience { get; set; }
}