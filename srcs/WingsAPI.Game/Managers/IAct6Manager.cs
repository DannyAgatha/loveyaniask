using WingsEmu.Game.Act6;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Managers
{
    public interface IAct6Manager
    {
        public Act6Faction AngelFaction { get; set; }
        public Act6Faction DemonFaction { get; set; }
        bool FactionPointsLocked { get; set; }
        void AddFactionPoints(FactionType factionType, int amount);
        void OpenEvents(FactionType factionType);
        void ResetFaction(FactionType factionType);
        Act6Status GetStatus();
    }

    public sealed record Act6Status(int AngelPointsPercentage, int AngelCurrentTime, int AngelTotalTime, byte AngelMode,
        int DemonPointsPercentage, int DemonCurrentTime, int DemonTotalTime, byte DemonMode);
}