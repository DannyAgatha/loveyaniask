using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act6
{
    public interface IAct6InstanceManager
    {
        // Audience
        public Act6InstanceObject Audience { get; set; }

        public void EnableAudience(FactionType faction);
        public void DisableAudience();

        // Pvp instance
        public Act6InstanceObject PvpInstance { get; set; }
        
        public IMapInstance? PvpMap { get; set; }

        public void EnablePvpInstance();
        public void DisablePvpInstance();
    }
}