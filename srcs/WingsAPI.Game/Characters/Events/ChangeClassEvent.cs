using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters.Events;

public class ChangeClassEvent : PlayerEvent
{
    public ClassType NewClass { get; set; }

    public bool ShouldObtainNewFaction { get; set; }

    public bool ShouldObtainBasicItems { get; set; }

    public bool ShouldResetJobLevel { get; set; }
}