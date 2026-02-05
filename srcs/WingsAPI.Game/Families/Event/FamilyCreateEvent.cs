// NosEmu
// 


using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyCreateEvent : PlayerEvent
{
    public string Name { get; set; }
}