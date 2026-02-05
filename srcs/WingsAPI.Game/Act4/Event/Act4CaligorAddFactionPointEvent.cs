using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event
{
    public class Act4CaligorAddFactionPointEvent : PlayerEvent
    {
        public Act4CaligorAddFactionPointEvent (FactionType faction, int damage)
        {
            Faction = faction;
            Damage = damage;
        }

        public FactionType Faction { get; set; }
        public int Damage { get; set; }
    }
}